namespace ApplicationModels.Models.AccountViewModels.Constants {
    public enum YearApPermissionLevels {
        Admin,
        Editor,
        ReadOnly
    }

    public static class Contansts {
        public static class Permissions {
            /**
               These strings are to be used on the method attribute Authorize that decorate a controller's endpoint.!--
               There should be one constnat entry for each value on the enum type YearApPermissionLevels

               Example:
                [Authorize(Roles = WebApp.Contansts.Permissions.Admin)]
             */
            public const string Admin = "Admin";
            public const string Editor = "Editor";
            public const string ReadOnly = "ReadOnly";
        }
    }
}
