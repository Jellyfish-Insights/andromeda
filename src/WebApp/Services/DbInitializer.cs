using System.Threading.Tasks;
using ApplicationModels;
using Microsoft.Extensions.Configuration;
using ApplicationModels.Models.AccountViewModels.Constants;
using ApplicationModels.Models.DataViewModels;
using System;
using Common;

namespace WebApp.Services {
    public class DbInitializer : IDbInitializer {
        ApplicationDbContext _dbContext;
        IAccountManagementService _accountManager;
        IConfiguration _configuration;
        IEmailService _emailService;

        public DbInitializer(
            ApplicationDbContext dbContext,
            IAccountManagementService accountManager,
            IConfiguration configuration,
            IEmailService emailService) {
            _dbContext = dbContext;
            _configuration = configuration;
            _accountManager = accountManager;
            _emailService = emailService;
        }

        public void Initialize() {
            CreateDefaultUser();
        }

        private void CreateDefaultUser() {
            var email = _configuration["DefaultUserEmail"];
            using (var transaction = _dbContext.Database.BeginTransaction()) {
                try {
                    if (_accountManager.CreateUser(email) &&
                        _accountManager.CreateDefaultRoles() &&
                        _accountManager.SetUserRole(
                            email,
                            new SingleAccountEdit(){
                        Permission = YearApPermissionLevels.Admin,
                        VersionStamp = _accountManager.GetAccountInfoByEmail(email).LastUpdate
                    })) {
                        transaction.Commit();
                        _emailService.SendMessage(email,
                                                  _configuration["WelcomeEmail:Subject"],
                                                  _configuration["WelcomeEmail:Text"]);
                    } else {
                        transaction.Rollback();
                    }
                }catch (Exception e) {
                    Console.WriteLine(e.Message);
                    transaction.Rollback();
                }
            }
        }
    }
}
