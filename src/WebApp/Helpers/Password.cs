// Based on https://stackoverflow.com/a/38997554/1352240
using System;
using System.Security.Cryptography;
using ApplicationModels.Models;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Helpers {
    public static class Password {
        private static readonly char[] ValidCharacters = "!@#$%^&*()_-+=[{]};:>|./?abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        public static string Generate(UserManager<ApplicationUser> manager, ApplicationUser user, int length) {
            if (length < 1 || length > 128) {
                throw new ArgumentException(nameof(length));
            }

            using (var rng = RandomNumberGenerator.Create()) {
                var byteBuffer = new byte[length];
                var characterBuffer = new char[length];
                var validator = new PasswordValidator<ApplicationUser>();
                var isValid = false;
                var password = "";

                do {
                    rng.GetBytes(byteBuffer);

                    for (var iter = 0; iter < length; iter++) {
                        var i = byteBuffer[iter] % ValidCharacters.Length;

                        characterBuffer[iter] = ValidCharacters[i];
                    }
                    password = new string(characterBuffer);
                    var isValidTask = validator.ValidateAsync(manager, user, password);
                    isValidTask.Wait();
                    isValid = isValidTask.Result.Succeeded;
                } while (!isValid);

                return password;
            }
        }
    }
}
