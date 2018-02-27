using ApplicationModels.Models.DataViewModels;

namespace WebApp.Services {
    public interface IAccountManagementService {
        bool CreateUser(string userEmail);
        bool RemoveUserByEmail(string userEmail);
        bool CreateDefaultRoles();
        bool SetUserRole(string userEmail, SingleAccountEdit edit);
        AllAccountsInfo ListAllUsers();
        AccountInfo GetAccountInfoByEmail(string userEmail);
    }
}
