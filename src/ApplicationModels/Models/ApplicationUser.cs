using Microsoft.AspNetCore.Identity;
using System;

namespace ApplicationModels.Models {
    public class ApplicationUser : IdentityUser {
        public DateTime? LastLoginTime { get; set; }
        public DateTime RegistrationDate { get; set; }

        /**
           Track last update trigger by user. This does not include
           update to LastLoginTime, which are triggered internally.
         */
        public DateTime? LastUpdate { get; set; }
    }
}
