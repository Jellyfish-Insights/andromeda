using System;
using System.Collections.Generic;
using ApplicationModels.Models.AccountViewModels.Constants;

namespace ApplicationModels.Models.DataViewModels {
    using UserEmail = System.String;
    public class SingleAccountEdit {
        public YearApPermissionLevels Permission { get; set; }
        public DateTime? VersionStamp { get; set; }
        public EditType Flag;
    }

    public class AccountEdit {
        public Dictionary<UserEmail, SingleAccountEdit> Edits { get; set; }

        public AccountEdit() {
            Edits = new Dictionary<UserEmail, SingleAccountEdit>();
        }
    }

    public class AccountInfo {
        public YearApPermissionLevels Role { get; set; }
        public DateTime? LastLogIn { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastUpdate { get; set; }
    }

    public class AuthStateInfo {
        public string Name { get; set; }
        public YearApPermissionLevels Role { get; set; }
    }

    public class AllAccountsInfo {
        public Dictionary<UserEmail, AccountInfo> Accounts;
    }
}
