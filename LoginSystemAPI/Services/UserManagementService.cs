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
        private string hostname;
        private IMailService mailHelper;
        public UserManagementService(IUnitOfWork unitOfWork, string secretKey, string hostname, IMailService mailHelper)
        {
            this.unitOfWork = unitOfWork;
            this.secretKey = secretKey;
            this.hostname = hostname;
            this.mailHelper = mailHelper;
        }
        public OperationResponse CreateUser(string email, string password)
        {
            OperationResponse response;
            if (string.IsNullOrWhiteSpace(password))
            {
                response = new OperationResponse()
                {
                    IsSuccess = false,
                    Message = Resources.Messages.EmptyPasswordError
                };
            }
            else
            {
                response = UserOperation(email,
                (repo, user) =>
                {
                    return new OperationResponse()
                    {
                        IsSuccess = false,
                        Message = Resources.Messages.CreateUserError
                    };
                },
                (repo) =>
                {
                    var (salt, encryptedPassword) = HashHelper.GetEncryptedPassword(password, secretKey);
                    repo.Insert(new User()
                    {
                        Email = email,
                        Password = encryptedPassword,
                        Salt = salt
                    });
                    unitOfWork.Save();
                    return new OperationResponse()
                    {
                        IsSuccess = true,
                        Message = Resources.Messages.CreateUserSuccess
                    };
                });
            }
            return response;
        }

        public OperationResponse RemoveUser(string email)
        {
            return UserOperation(email,
                (repo, user) =>
                {
                    repo.Delete(user);
                    unitOfWork.Save();
                    return new OperationResponse()
                    {
                        IsSuccess = true,
                        Message = Resources.Messages.RemoveUserSuccess
                    };
                },
                (repo) =>
                {
                    return new OperationResponse()
                    {
                        IsSuccess = false,
                        Message = Resources.Messages.UserNotFoundError
                    };
                });
        }

        public OperationResponse UpdateUser(string email, string password)
        {
            OperationResponse response;
            if (string.IsNullOrWhiteSpace(password))
            {
                response = new OperationResponse()
                {
                    IsSuccess = false,
                    Message = Resources.Messages.EmptyPasswordError
                };
            }
            else
            {
                response = UserOperation(email,
                (repo, user) =>
                {
                    var (salt, encryptedPassword) = HashHelper.GetEncryptedPassword(password, secretKey);
                    user.Password = encryptedPassword;
                    user.Salt = salt;
                    repo.Update(user);
                    unitOfWork.Save();
                    return new OperationResponse()
                    {
                        IsSuccess = true,
                        Message = Resources.Messages.UpdateUserSuccess
                    };
                },
                (repo) =>
                {
                    return new OperationResponse()
                    {
                        IsSuccess = false,
                        Message = Resources.Messages.UserNotFoundError
                    };
                });
            }
            return response;
        }

        public OperationResponse InitiateForgotPassword(string email)
        {
            return UserOperation(email,
                (repo, user) =>
                {
                    var passwordResetRepo = unitOfWork.GetRepository<PasswordReset>();
                    var resetKey = Guid.NewGuid();
                    passwordResetRepo.Insert(new PasswordReset()
                    {
                        DateRequested = DateTime.UtcNow,
                        User = user,
                        Key = resetKey
                    });
                    unitOfWork.Save();
                    var passwordResetLink = $"{hostname}/User/ResetPassword/{resetKey}";
                    mailHelper.SendMail(user.Email, "Example.com - Password Reset Request", $@"<html><body>Hello,<br/><br/>A password reset has been initiated for your account. Please click <a href=""{passwordResetLink}"">here</a> to reset your password.<br/>Thanks,<br/>Admin Team</body></html>");
                    return new OperationResponse()
                    {
                        IsSuccess = true,
                        Message = Resources.Messages.InitiateForgotPasswordSuccess
                    };
                },
                (repo) =>
                {
                    return new OperationResponse()
                    {
                        IsSuccess = false,
                        Message = Resources.Messages.UserNotFoundError
                    };
                });
        }

        public OperationResponse ForgotPasswordReset(Guid key, string password)
        {
            OperationResponse response;
            if (string.IsNullOrWhiteSpace(password))
            {
                response = new OperationResponse()
                {
                    IsSuccess = false,
                    Message = Resources.Messages.EmptyPasswordError
                };
            }
            else
            {
                var passwordResetRepo = unitOfWork.GetRepository<PasswordReset>();
                var passwordReset = passwordResetRepo.Get(x => x.Key == key).FirstOrDefault();
                if (passwordReset != null)
                {
                    if (DateTime.UtcNow.AddMinutes(-15) > passwordReset.DateRequested)
                    {
                        response = new OperationResponse()
                        {
                            IsSuccess = false,
                            Message = Resources.Messages.ForgotPasswordResetExpired
                        };
                    }
                    else
                    {
                        response = UpdateUser(passwordReset.User.Email, password);
                    }
                }
                else
                {
                    response = new OperationResponse()
                    {
                        IsSuccess = false,
                        Message = Resources.Messages.KeyNotFoundError
                    };
                }
            }
            return response;
        }

        private OperationResponse UserOperation(string email, Func<IRepository<User>, User, OperationResponse> userFunc, Func<IRepository<User>, OperationResponse> noUserFunc)
        {
            OperationResponse response;
            if (string.IsNullOrWhiteSpace(email))
            {
                response = new OperationResponse()
                {
                    IsSuccess = false,
                    Message = Resources.Messages.EmptyEmailError
                };
            }
            else
            {
                var users = unitOfWork.GetRepository<User>();
                User existingUser = users.Get(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (existingUser != null)
                {
                    response = userFunc(users, existingUser);
                }
                else
                {
                    response = noUserFunc(users);
                }
            }
            return response;
        }


    }
}