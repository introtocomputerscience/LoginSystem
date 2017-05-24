using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomLoginSystem.Resources
{
    public class Messages
    {
        public const string CreateUserError = "A user already exists with the requested email.";
        public const string EmptyEmailError = "Email is null or empty.";
        public const string EmptyPasswordError = "Password is null or empty.";
        public const string CreateUserSuccess = "User was successfully created.";
        public const string RemoveUserSuccess = "User was successfully removed.";
        public const string UpdateUserSuccess = "User was successfully updated.";
        public const string LoginSuccess = "Access Granted";
        public const string LoginFailure = "Authorization Denied";
        public const string InitiateForgotPasswordSuccess = "Password reset initiated.";
        public const string ForgotPasswordResetExpired = "Password reset has expired.";
        public const string UserNotFoundError = "A user was not found with the requested email.";
        public const string KeyNotFoundError = "A password reset was not found with the requested key.";
        public const string Unauthorized = "Unauthorized.";
    }
}