using System.Threading.Tasks;
using System.Linq;
using System;
using ApplicationModels.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ApplicationModels.Models.AccountViewModels.Constants;
using ApplicationModels.Models.DataViewModels;
using ApplicationModels;

namespace WebApp.Services {
    public class AccountManagementService : IAccountManagementService {

        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        IConfiguration _configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;
        ApplicationDbContext ApContext;
        ILogger<AccountManagementService> _logger;

        public AccountManagementService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountManagementService> logger,
            IConfiguration configuration,
            ApplicationDbContext context,
            SignInManager<ApplicationUser> signInManager
            ) {
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._configuration = configuration;
            this.ApContext = context;
            this._signInManager = signInManager;
            this._logger = logger;
        }

        public bool CreateUser(string userEmail) {
            var user = _userManager.FindByEmailAsync(userEmail).Result;
            if (user != null) {
                _logger.LogError("User {} already exists", userEmail);
                return false;
            }
            return _userManager.CreateAsync(new ApplicationUser {
                UserName = userEmail,
                Email = userEmail,
                RegistrationDate = DateTime.UtcNow,
                LastUpdate = DateTime.MinValue
            }).Result.Succeeded;
        }

        private bool CreateRole(YearApPermissionLevels role) {
            var roleName = role.ToString();

            if (_roleManager.RoleExistsAsync(roleName).Result) {
                _logger.LogWarning("Role {} already exists", roleName);
                return false;
            }

            return _roleManager.CreateAsync(new IdentityRole(roleName)).Result.Succeeded;
        }

        public bool CreateDefaultRoles() {
            CreateRole(YearApPermissionLevels.Admin);
            CreateRole(YearApPermissionLevels.Editor);
            CreateRole(YearApPermissionLevels.ReadOnly);
            return true;
        }

        /**
           From the point of view of the UI there are three hierarchical levels: Admin, Association Manager, and Read-only.

           This is implemented by use of roles on Asp.Net Core's. However, roles on Asp.Net are not hierarchical.

           Hierarchy is achieved by adding to an account all roles which are equal or less priviledged than its UI role.

           For example: if an account is set to bui an admin by the UI, this account shall have the roles: admin, manager, and read only.
         */
        private static YearApPermissionLevels[] GetAspNetRolesForUiRole(YearApPermissionLevels permission) {
            switch (permission) {
                case YearApPermissionLevels.Admin:
                    return new YearApPermissionLevels[] {
                               YearApPermissionLevels.Admin,
                               YearApPermissionLevels.Editor,
                               YearApPermissionLevels.ReadOnly
                    };
                case YearApPermissionLevels.Editor:
                    return new YearApPermissionLevels[] {
                               YearApPermissionLevels.Editor,
                               YearApPermissionLevels.ReadOnly
                    };
                case YearApPermissionLevels.ReadOnly:
                    return new YearApPermissionLevels[] {
                               YearApPermissionLevels.ReadOnly
                    };
                default:
                    throw new ArgumentException($"Invalid permission {permission}");
            }
        }

        public bool SetUserRole(string userEmail, SingleAccountEdit edit) {
            var user = _userManager.FindByEmailAsync(userEmail).Result;
            if (user == null) {
                throw new ArgumentException($"Cannot set role for non existing user '{userEmail}'");
            }
            if (user.LastUpdate.Value.CompareTo(edit.VersionStamp.Value) != 0) {
                _logger.LogWarning("Invalid timestamp when editing role of user {}", userEmail);
                _logger.LogDebug("Most recent: {}; attempted: {}", user.LastUpdate.Value.Ticks, edit.VersionStamp.Value.Ticks);
                return false;
            }
            var succeededClearing = ClearUserRoles(user);
            var aspNetRoles = GetAspNetRolesForUiRole(edit.Permission);
            var hadErrorAddingRoles = aspNetRoles.Select(r => AddAspNetRoleToUser(r, user)).Where(b => !b).Any();
            user.LastUpdate = DateTime.UtcNow;
            _userManager.UpdateSecurityStampAsync(user).Wait();
            return succeededClearing && !hadErrorAddingRoles && _userManager.UpdateAsync(user).Result.Succeeded;
        }

        private bool AddAspNetRoleToUser(YearApPermissionLevels role, ApplicationUser user) {
            var roleName = role.ToString();
            if (_userManager.IsInRoleAsync(user, roleName).Result) {
                _logger.LogWarning("User already in role {}", roleName);
                return false;
            }
            return _userManager.AddToRoleAsync(user, roleName).Result.Succeeded;
        }

        private bool RemoveRoleFromUser(YearApPermissionLevels role, ApplicationUser user) {
            var roleName = role.ToString();
            if (!_userManager.IsInRoleAsync(user, roleName).Result) {
                _logger.LogWarning("User not in role {}", roleName);
                return false;
            }
            if (role == YearApPermissionLevels.Admin) {
                if (_userManager.GetUsersInRoleAsync(role.ToString()).Result.Count() == 1) {
                    throw new InvalidOperationException("There must be at least one admin user!");
                }
            }
            return _userManager.RemoveFromRoleAsync(user, roleName).Result.Succeeded;
        }

        private YearApPermissionLevels ParseRole(string roleName) {
            return (YearApPermissionLevels) Enum.Parse(typeof(YearApPermissionLevels), roleName);
        }

        private bool ClearUserRoles(ApplicationUser user) {
            var roles = _userManager.GetRolesAsync(user).Result;
            var hasFailure = roles.Select(x => RemoveRoleFromUser(ParseRole(x), user)).Where(b => !b).Any();
            return !hasFailure;
        }

        private YearApPermissionLevels GetRoleOfUser(ApplicationUser user) {
            if (_userManager.IsInRoleAsync(user, YearApPermissionLevels.Admin.ToString()).Result) {
                return YearApPermissionLevels.Admin;
            } else if (_userManager.IsInRoleAsync(user, YearApPermissionLevels.Editor.ToString()).Result) {
                return YearApPermissionLevels.Editor;
            } else
                return YearApPermissionLevels.ReadOnly;
        }

        private AllAccountsInfo GetAccounts(IEnumerable<ApplicationUser> accounts) {
            return new AllAccountsInfo(){
                       Accounts = (from user in accounts
                                   select new {
                    Email = user.Email,
                    Permission = GetRoleOfUser(user),
                    RegistrationDate = user.RegistrationDate,
                    LastLogin = user.LastLoginTime,
                    LastUpdate = user.LastUpdate
                }).ToDictionary(x => x.Email, x => new AccountInfo(){
                    Role = x.Permission,
                    LastLogIn = x.LastLogin,
                    RegistrationDate = x.RegistrationDate,
                    LastUpdate = x.LastUpdate
                })
            };
        }

        public AllAccountsInfo ListAllUsers() {
            var allAccounts = _userManager.Users.ToList();
            return GetAccounts(allAccounts);
        }

        public AccountInfo GetAccountInfoByEmail(string userEmail) {
            var user = _userManager.FindByEmailAsync(userEmail).Result;
            return new AccountInfo(){
                       Role = GetRoleOfUser(user),
                       LastLogIn = user.LastLoginTime,
                       RegistrationDate = user.RegistrationDate,
                       LastUpdate = user.LastUpdate
            };
        }

        public bool RemoveUserByEmail(string userEmail) {
            var user = _userManager.FindByEmailAsync(userEmail).Result;
            if (user == null) {
                return false;
            }
            _userManager.UpdateSecurityStampAsync(user).Wait();
            return ClearUserRoles(user) && _userManager.DeleteAsync(user).Result.Succeeded;
        }
    }
}
