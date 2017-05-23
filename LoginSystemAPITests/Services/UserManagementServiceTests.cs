using Microsoft.VisualStudio.TestTools.UnitTesting;
using CustomLoginSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using LoginSystemDAL.Interfaces;
using LoginSystemDAL;
using CustomLoginSystem.Helpers;
using System.Linq.Expressions;
using CustomLoginSystem.Interfaces;

namespace CustomLoginSystem.Services.Tests
{
    [TestClass()]
    public class UserManagementServiceTests
    {
        private UserManagementService service;
        private Mock<IRepository<User>> userRepository;
        private Mock<IRepository<PasswordReset>> passwordResetRepository;
        private Mock<IMailService> mailService;
        private string secretKey = "secretKey";
        private string hostname = "https://localhost";

        [TestInitialize]
        public void InitializeTest()
        {
            var unitOfWork = new Mock<IUnitOfWork>();
            userRepository = new Mock<IRepository<User>>();
            passwordResetRepository = new Mock<IRepository<PasswordReset>>();
            mailService = new Mock<IMailService>();

            unitOfWork.Setup(x => x.GetRepository<User>()).Returns(userRepository.Object);
            unitOfWork.Setup(x => x.GetRepository<PasswordReset>()).Returns(passwordResetRepository.Object);

            this.service = new UserManagementService(unitOfWork.Object, secretKey, hostname, mailService.Object);
        }

        [TestMethod()]
        public void CreateUserWithNewEmailTest()
        {
            var email = "test@test.com";
            var password = "samplePassword";

            var response = service.CreateUser(email, password);

            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.CreateUserSuccess, response.Message);
        }

        [TestMethod()]
        public void CreateUserWithEmptyEmailTest()
        {
            var email = "";
            var password = "samplePassword";

            var response = service.CreateUser(email, password);

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.EmptyEmailError, response.Message);
        }

        [TestMethod()]
        public void CreateUserWithEmptyPasswordTest()
        {
            var email = "test@test.com";
            var password = "";

            var response = service.CreateUser(email, password);

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.EmptyPasswordError, response.Message);
        }

        [TestMethod()]
        public void CreateUserWithExistingEmailTest()
        {
            var email = "test@test.com";
            var password = "samplePassword";

            var salt = Guid.NewGuid();
            var saltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt.ToString()));
            var encryptedPassword = HashHelper.ComputeHash(saltedPassword, Encoding.UTF8.GetBytes(secretKey));

            var user = new User()
            {
                Email = email,
                Password = encryptedPassword,
                Salt = salt
            };
            userRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<string>())).Returns(new List<User> { user });

            var response = service.CreateUser(email, password);

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.CreateUserError, response.Message);
        }

        [TestMethod()]
        public void RemoveUserWithExistingEmailTest()
        {
            var email = "test@test.com";
            var password = "samplePassword";

            var salt = Guid.NewGuid();
            var saltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt.ToString()));
            var encryptedPassword = HashHelper.ComputeHash(saltedPassword, Encoding.UTF8.GetBytes(secretKey));

            var user = new User()
            {
                Email = email,
                Password = encryptedPassword,
                Salt = salt
            };

            userRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<string>())).Returns(new List<User> { user });

            var response = service.RemoveUser(email);

            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.RemoveUserSuccess, response.Message);
        }

        [TestMethod()]
        public void RemoveUserWithNonExistingEmailTest()
        {
            var email = "test@test.com";

            var response = service.RemoveUser(email);

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.UserNotFoundError, response.Message);
        }

        [TestMethod()]
        public void UpdateUserWithExistingEmailTest()
        {
            var email = "test@test.com";
            var password = "samplePassword";
            var newPassword = "newPassword";

            var salt = Guid.NewGuid();
            var saltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt.ToString()));
            var encryptedPassword = HashHelper.ComputeHash(saltedPassword, Encoding.UTF8.GetBytes(secretKey));

            var user = new User()
            {
                Email = email,
                Password = encryptedPassword,
                Salt = salt
            };
            userRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<string>())).Returns(new List<User> { user });

            var response = service.UpdateUser(email, "newPassword");
            User modifiedUser = userRepository.Object.Get().FirstOrDefault();
            var newSaltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(newPassword), Encoding.UTF8.GetBytes(modifiedUser.Salt.ToString()));
            var newEncryptedPassword = HashHelper.ComputeHash(newSaltedPassword, Encoding.UTF8.GetBytes(secretKey));

            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.UpdateUserSuccess, response.Message);
            Assert.IsNotNull(modifiedUser);
            Assert.IsFalse(modifiedUser.Password.SequenceEqual(encryptedPassword));
            Assert.IsTrue(modifiedUser.Password.SequenceEqual(newEncryptedPassword));
            Assert.AreNotEqual(salt, modifiedUser.Salt);
        }

        [TestMethod()]
        public void UpdateUserWithNonExistingEmailTest()
        {
            var email = "test@test.com";

            var response = service.UpdateUser(email, "newPassword");

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.UserNotFoundError, response.Message);
        }

        [TestMethod()]
        public void LoginWithValidDetailsTest()
        {
            var email = "test@test.com";
            var password = "samplePassword";

            var salt = Guid.NewGuid();
            var saltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt.ToString()));
            var encryptedPassword = HashHelper.ComputeHash(saltedPassword, Encoding.UTF8.GetBytes(secretKey));

            var user = new User()
            {
                Email = email,
                Password = encryptedPassword,
                Salt = salt
            };
            userRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<string>())).Returns(new List<User> { user });

            var response = service.Login(email, password);

            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.LoginSuccess, response.Message);
        }

        [TestMethod()]
        public void LoginWithInvalidDetailsTest()
        {
            var email = "test@test.com";
            var password = "samplePassword";

            var salt = Guid.NewGuid();
            var saltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt.ToString()));
            var encryptedPassword = HashHelper.ComputeHash(saltedPassword, Encoding.UTF8.GetBytes(secretKey));

            var user = new User()
            {
                Email = email,
                Password = encryptedPassword,
                Salt = salt
            };
            userRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<string>())).Returns(new List<User> { user });

            var response = service.Login(email, "invalidPassword");

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.LoginFailure, response.Message);
        }

        [TestMethod()]
        public void LoginWithNonExistingEmailTest()
        {
            var email = "test@test.com";
            var password = "samplePassword";

            var response = service.Login(email, password);

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.UserNotFoundError, response.Message);
        }

        [TestMethod()]
        public void InitiateForgotPasswordWithExistingEmailTest()
        {
            var email = "test@test.com";
            var password = "samplePassword";

            var salt = Guid.NewGuid();
            var saltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt.ToString()));
            var encryptedPassword = HashHelper.ComputeHash(saltedPassword, Encoding.UTF8.GetBytes(secretKey));

            var user = new User()
            {
                Email = email,
                Password = encryptedPassword,
                Salt = salt
            };
            userRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<string>())).Returns(new List<User> { user });

            var response = service.InitiateForgotPassword(email);

            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.InitiateForgotPasswordSuccess, response.Message);
        }

        [TestMethod()]
        public void InitiateForgotPasswordWithNonExistingEmailTest()
        {
            var email = "test@test.com";

            var response = service.InitiateForgotPassword(email);

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.UserNotFoundError, response.Message);
        }

        [TestMethod()]
        public void ForgotPasswordResetWithExistingKeyTest()
        {
            var email = "test@test.com";
            var password = "samplePassword";
            var key = Guid.NewGuid();
            var newPassword = "newPassword";

            var salt = Guid.NewGuid();
            var saltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt.ToString()));
            var encryptedPassword = HashHelper.ComputeHash(saltedPassword, Encoding.UTF8.GetBytes(secretKey));

            var user = new User()
            {
                Email = email,
                Password = encryptedPassword,
                Salt = salt
            };
            userRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<string>())).Returns(new List<User> { user });
            var passwordReset = new PasswordReset()
            {
                Key = key,
                User = user,
                DateRequested = DateTime.UtcNow.AddMinutes(-10)
            };
            passwordResetRepository.Setup(x => x.Get(It.IsAny<Expression<Func<PasswordReset, bool>>>(), It.IsAny<Func<IQueryable<PasswordReset>, IOrderedQueryable<PasswordReset>>>(), It.IsAny<string>())).Returns(new List<PasswordReset> { passwordReset });

            var response = service.ForgotPasswordReset(key, newPassword);
            User modifiedUser = userRepository.Object.Get().FirstOrDefault();
            var newSaltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(newPassword), Encoding.UTF8.GetBytes(modifiedUser.Salt.ToString()));
            var newEncryptedPassword = HashHelper.ComputeHash(newSaltedPassword, Encoding.UTF8.GetBytes(secretKey));

            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.UpdateUserSuccess, response.Message);
            Assert.IsNotNull(modifiedUser);
            Assert.AreEqual(email, modifiedUser.Email);
            Assert.IsFalse(modifiedUser.Password.SequenceEqual(encryptedPassword));
            Assert.IsTrue(modifiedUser.Password.SequenceEqual(newEncryptedPassword));
            Assert.AreNotEqual(salt, modifiedUser.Salt);
        }

        [TestMethod()]
        public void ForgotPasswordResetWithExpiredKeyTest()
        {
            var email = "test@test.com";
            var password = "samplePassword";
            var key = Guid.NewGuid();
            var newPassword = "newPassword";

            var salt = Guid.NewGuid();
            var saltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt.ToString()));
            var encryptedPassword = HashHelper.ComputeHash(saltedPassword, Encoding.UTF8.GetBytes(secretKey));

            var user = new User()
            {
                Email = email,
                Password = encryptedPassword,
                Salt = salt
            };
            userRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<string>())).Returns(new List<User> { user });
            var passwordReset = new PasswordReset()
            {
                Key = key,
                User = user,
                DateRequested = DateTime.UtcNow.AddMinutes(-20)
            };
            passwordResetRepository.Setup(x => x.Get(It.IsAny<Expression<Func<PasswordReset, bool>>>(), It.IsAny<Func<IQueryable<PasswordReset>, IOrderedQueryable<PasswordReset>>>(), It.IsAny<string>())).Returns(new List<PasswordReset> { passwordReset });

            var response = service.ForgotPasswordReset(key, newPassword);
            User modifiedUser = userRepository.Object.Get().FirstOrDefault();
            var newSaltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(newPassword), Encoding.UTF8.GetBytes(modifiedUser.Salt.ToString()));
            var newEncryptedPassword = HashHelper.ComputeHash(newSaltedPassword, Encoding.UTF8.GetBytes(secretKey));

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.ForgotPasswordResetExpired, response.Message);
            Assert.IsNotNull(modifiedUser);
            Assert.AreEqual(email, modifiedUser.Email);
            Assert.IsTrue(modifiedUser.Password.SequenceEqual(encryptedPassword));
            Assert.IsFalse(modifiedUser.Password.SequenceEqual(newEncryptedPassword));
            Assert.AreEqual(salt, modifiedUser.Salt);
        }

        [TestMethod()]
        public void ForgotPasswordResetWithNonExistingKeyTest()
        {
            var email = "test@test.com";
            var password = "samplePassword";
            var key = Guid.NewGuid();
            var newPassword = "newPassword";

            var salt = Guid.NewGuid();
            var saltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt.ToString()));
            var encryptedPassword = HashHelper.ComputeHash(saltedPassword, Encoding.UTF8.GetBytes(secretKey));

            var user = new User()
            {
                Email = email,
                Password = encryptedPassword,
                Salt = salt
            };
            userRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(), It.IsAny<string>())).Returns(new List<User> { user });

            var response = service.ForgotPasswordReset(key, newPassword);
            User modifiedUser = userRepository.Object.Get().FirstOrDefault();
            var newSaltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(newPassword), Encoding.UTF8.GetBytes(modifiedUser.Salt.ToString()));
            var newEncryptedPassword = HashHelper.ComputeHash(newSaltedPassword, Encoding.UTF8.GetBytes(secretKey));

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.KeyNotFoundError, response.Message);
            Assert.IsNotNull(modifiedUser);
            Assert.AreEqual(email, modifiedUser.Email);
            Assert.IsTrue(modifiedUser.Password.SequenceEqual(encryptedPassword));
            Assert.IsFalse(modifiedUser.Password.SequenceEqual(newEncryptedPassword));
            Assert.AreEqual(salt, modifiedUser.Salt);
        }
    }
}