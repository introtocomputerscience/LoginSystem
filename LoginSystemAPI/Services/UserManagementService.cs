using CustomLoginSystem.Helpers;
using CustomLoginSystem.Interfaces;
using CustomLoginSystem.Models;
using LoginSystemDAL;
using LoginSystemDAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CustomLoginSystem.Services
{
    public class UserManagementService : IUserManagementService
    {
        private IUnitOfWork unitOfWork;
        private string secretKey;
        public UserManagementService(IUnitOfWork unitOfWork, string secretKey)
        {
            this.unitOfWork = unitOfWork;
            this.secretKey = secretKey;
        }
        public OperationResponse CreateUser(string email, string password)
        {
            OperationResponse response;
            var users = unitOfWork.GetRepository<User>();
            User existingUser = users.Get(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (existingUser != null)
            {
                response = new OperationResponse()
                {
                    IsSuccess = false,
                    Message = Resources.Messages.CreateUserError
                };
            }
            else
            {
                var salt = Guid.NewGuid();
                var saltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt.ToString()));
                var encryptedPassword = HashHelper.ComputeHash(saltedPassword, Encoding.UTF8.GetBytes(secretKey));
                users.Insert(new User()
                {
                    Email = email,
                    Password = encryptedPassword,
                    Salt = salt
                });
                response = new OperationResponse()
                {
                    IsSuccess = true,
                    Message = Resources.Messages.CreateUserSuccess
                };
            }
            return response;
        }

        public OperationResponse RemoveUser(string email)
        {
            return new OperationResponse();
        }

        public OperationResponse UpdateUser(string email, string password)
        {
            return new OperationResponse();
        }

        public OperationResponse Login(string email, string password)
        {
            return new OperationResponse();
        }

        public OperationResponse InitiateForgotPassword(string email)
        {
            return new OperationResponse();
        }

        public OperationResponse ForgotPasswordReset(Guid key, string password)
        {
            return new OperationResponse();
        }
    }
}