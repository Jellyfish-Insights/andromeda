using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ApplicationModels;
using ApplicationModels.Models;
using ApplicationModels.Models.AccountViewModels;
using ApplicationModels.Models.AccountViewModels.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApp.Services;
using ApplicationModels.Models.DataViewModels;
using System;
using Common;

namespace WebApp.Controllers {
    [Authorize(Roles = Contansts.Permissions.ReadOnly)]
    [Route("api/[controller]/[action]")]
    public class AccountController : Controller {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IAccountManagementService _accountManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        private const string AccessDeniedPath = "/account/denied";
        private const string FeeEmailDomain = "@fee.org";

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IHostingEnvironment hostingEnvironment,
            IAccountManagementService accountManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dbContext,
            IEmailService emailService,
            IConfiguration configuration) {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _roleManager = roleManager;
            _accountManager = accountManager;
            _dbContext = dbContext;
            _emailService = emailService;
            _configuration = configuration;
        }

        #region Authenticated End Points

        [HttpPost]
        public IActionResult Logout() {
            var user = _userManager.FindByEmailAsync(User.Identity.Name).Result;
            _userManager.UpdateSecurityStampAsync(user).Wait();
            _signInManager.SignOutAsync().Wait();
            return RedirectToAction(nameof(Lockout));
        }

        [HttpGet]
        public AccountInfo GetRole() {
            return _accountManager.GetAccountInfoByEmail(User.Identity.Name);
        }

        #endregion

        #region AllowAnonymous End Points

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AuthState() {
            #if NOAUTH
            return Ok(new AuthStateInfo(){
                Name = "Apple",
                Role = YearApPermissionLevels.Admin
            });
            #else
            var result = User.Identity.IsAuthenticated && _signInManager.IsSignedIn(User);
            if (result) {
                var accountInfo = _accountManager.GetAccountInfoByEmail(User.Identity.Name);
                return Ok(new AuthStateInfo(){
                    Name = User.Identity.Name,
                    Role = accountInfo.Role
                });
            } else {
                return Unauthorized();
            }
            #endif
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout() {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null) {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null) {
            if (remoteError != null) {
                _logger.LogError($"Error from external provider: {remoteError}");
                return Redirect(AccessDeniedPath);
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) {
                _logger.LogWarning("External provider returned no info");
                return Redirect(AccessDeniedPath);
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent : false, bypassTwoFactor : true);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = _userManager.FindByEmailAsync(email).Result;
            if (result.Succeeded) {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                user.LastLoginTime = DateTime.UtcNow;
                _userManager.UpdateAsync(user).Wait();
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut) {
                _logger.LogWarning($"User {email} is locked out");
                return Redirect(AccessDeniedPath);
            }
            // if the user doesn't exist but his email is @fee.org, create the user
            var emailCreated = true;
            if (user == null && email.EndsWith(FeeEmailDomain)) {
                _logger.LogInformation($"Creating read-only user for email {email}");
                emailCreated = _accountManager.CreateUser(email);
                user = _userManager.FindByEmailAsync(email).Result;
                var editOperation = new SingleAccountEdit() {
                    Permission = YearApPermissionLevels.ReadOnly,
                    VersionStamp = user.LastUpdate,
                    Flag = EditType.New,
                };
                emailCreated = emailCreated && _accountManager.SetUserRole(email, editOperation);
                var emailSending = _emailService.SendMessage(email,
                                                             _configuration.GetValue<string>("WelcomeEmail:Subject", ""),
                                                             _configuration.GetValue<string>("WelcomeEmail:Text", ""));
                if (!emailSending.IsSuccessful) {
                    _logger.LogError($"Tried to send an email to {email}, but it failed with {emailSending.ErrorMessage}");
                }
            }
            // if the user exists but didn't succeed, associate his account to the new provider
            if (user != null && emailCreated) {
                _logger.LogInformation($"Associating {email} with external provider");
                var associate = await _userManager.AddLoginAsync(user, info);
                if (associate.Succeeded) {
                    await _signInManager.SignInAsync(user, isPersistent : false);
                    _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                    user.LastLoginTime = DateTime.UtcNow;
                    _userManager.UpdateAsync(user).Wait();

                    return RedirectToLocal(returnUrl);
                }
            }
            _logger.LogWarning("User doesn't exist");
            // if the user doesn't exist, his access is denied
            return Redirect(AccessDeniedPath);
        }

        #endregion

        #region Admin End Points
        private bool ExecuteAccountEdit(string email, SingleAccountEdit editOperation) {
            switch (editOperation.Flag) {
                case EditType.New:
                    _logger.LogInformation("Creating user for email {} with permission {}", email, editOperation.Permission.ToString());
                    var emailCreated = _accountManager.CreateUser(email);
                    var user = _accountManager.GetAccountInfoByEmail(email);
                    editOperation.VersionStamp = user.LastUpdate;
                    var emailSending = _emailService.SendMessage(email,
                                                                 _configuration.GetValue<string>("WelcomeEmail:Subject", ""),
                                                                 _configuration.GetValue<string>("WelcomeEmail:Text", ""));
                    if (!emailSending.IsSuccessful) {
                        _logger.LogError($"Tried to send an email to {email}, but it failed with {emailSending.ErrorMessage}");
                    }
                    return emailCreated && _accountManager.SetUserRole(email, editOperation);
                case EditType.Update:
                    _logger.LogInformation("Setting role of user {} to {}", email, editOperation.Permission.ToString());
                    return _accountManager.SetUserRole(email, editOperation);
                case EditType.Delete:
                    _logger.LogInformation("Removing user with email {}", email);
                    return _accountManager.RemoveUserByEmail(email);
                default:
                    throw new Exception("Unsupported operation");
            }
        }

        [HttpPost]
        [Authorize(Roles = Contansts.Permissions.Admin)]

        /**
           Returns the edit operations that failed.
         */
        public AccountEdit EditAccount([FromBody] AccountEdit edits) {
            var errors = new AccountEdit();
            using (var transaction = _dbContext.Database.BeginTransaction()) {
                try {
                    foreach (var(email, editOperation) in edits.Edits) {
                        if (!ExecuteAccountEdit(email, editOperation)) {
                            errors.Edits[email] = editOperation;
                        }
                    }
                    transaction.Commit();
                }catch (Exception e) {
                    _logger.LogError(e, "Error while editing account! All updates discarded.");
                    transaction.Rollback();
                    return edits;
                }
            }
            return errors;
        }

        [HttpGet]
        [Authorize(Roles = Contansts.Permissions.ReadOnly)]
        public AllAccountsInfo ListAllAccountRoles() {
            return _accountManager.ListAllUsers();
        }

        #endregion

        #region Helpers

        private void AddErrors(IdentityResult result) {
            foreach (var error in result.Errors) {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl) {
            if (Url.IsLocalUrl(returnUrl)) {
                return Redirect(returnUrl);
            } else {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        private static YearApPermissionLevels ParseRole(string roleName) {
            return (YearApPermissionLevels) Enum.Parse(typeof(YearApPermissionLevels), roleName);
        }

        #endregion
    }
}
