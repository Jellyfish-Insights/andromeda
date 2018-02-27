using Xunit;
using System.Linq;
using WebApp.Services;
using Test.Helpers;
using WebApp;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using ApplicationModels.Models.AccountViewModels.Constants;
using Common;
using ApplicationModels.Models.AccountViewModels;
using ApplicationModels.Models.DataViewModels;

namespace Test {
    public class AccountManagementTest {
        public IAccountManagementService manager;
        public IWebHost WebHost;

        public AccountManagementTest() {
            DatabaseReset.Drop(Databases.AnalyticsPlatform);
            DatabaseReset.Migrate(Databases.AnalyticsPlatform);
            WebHost = Program.BuildWebHost(new string[] {});
            using (var scope = WebHost.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                services.GetService<IDbInitializer>().Initialize();
            }
        }

        #region Test Steps

        private YearApPermissionLevels GetUserRole(string userEmail) {
            using (var scope = WebHost.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                manager = services.GetService<IAccountManagementService>();
                return manager.GetAccountInfoByEmail(userEmail).Role;
            }
        }

        private AllAccountsInfo ListAllRoles() {
            using (var scope = WebHost.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                manager = services.GetService<IAccountManagementService>();
                return manager.ListAllUsers();
            }
        }

        private void ThereIsUserWithRole(string userEmail, YearApPermissionLevels role) {
            using (var scope = WebHost.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                manager = services.GetService<IAccountManagementService>();

                manager.CreateUser(userEmail);
                var accountInfo = manager.GetAccountInfoByEmail(userEmail);
                manager.SetUserRole(userEmail, new SingleAccountEdit(){ Permission = role, VersionStamp = accountInfo.LastUpdate });
            }
        }

        #endregion

        #region Test Scenarios

        [Fact]
        public void ThereExistsInitialAdminUser() {
            var roles = ListAllRoles().Accounts.AsEnumerable().ToList();
            Assert.Single(roles);
            Assert.Equal(YearApPermissionLevels.Admin, roles.Single().Value.Role);
        }

        [Fact]
        public void CorrectlyCreatesNewUserWithRole() {

            ThereIsUserWithRole(
                "newUser@fee.test",
                YearApPermissionLevels.Editor
                );

            var role = GetUserRole("newUser@fee.test");

            Assert.Equal(YearApPermissionLevels.Editor, role);
        }

        [Fact]
        public void CorrectlyRemovesRoleFromUser() {
            ThereIsUserWithRole(
                "newUser@fee.test",
                YearApPermissionLevels.Admin
                );
            var userRole = GetUserRole("newUser@fee.test");
            Assert.Equal(YearApPermissionLevels.Admin, userRole);

            ThereIsUserWithRole(
                "newUser@fee.test",
                YearApPermissionLevels.Editor
                );

            var newUserRole = GetUserRole("newUser@fee.test");
            Assert.Equal(YearApPermissionLevels.Editor, newUserRole);
        }

        #endregion
    }
}
