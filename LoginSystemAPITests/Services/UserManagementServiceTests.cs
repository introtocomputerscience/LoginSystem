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

namespace CustomLoginSystem.Services.Tests
{
    [TestClass()]
    public class UserManagementServiceTests
    {
        private UserManagementService service;
        private Mock<IRepository<User>> userRepository;
        private Mock<IRepository<PasswordReset>> passwordResetRepository;
        private string secretKey = "secretKey";

        [TestInitialize]
        public void InitializeTest()
        {
            var unitOfWork = new Mock<IUnitOfWork>();
            userRepository = new Mock<IRepository<User>>();
            passwordResetRepository = new Mock<IRepository<PasswordReset>>();

            unitOfWork.Setup(x => x.GetRepository<User>()).Returns(userRepository.Object);
            unitOfWork.Setup(x => x.GetRepository<PasswordReset>()).Returns(passwordResetRepository.Object);

            this.service = new UserManagementService(unitOfWork.Object, secretKey);
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
            userRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User,bool>>>(), It.IsAny<Func<IQueryable<User>,IOrderedQueryable<User>>>(), It.IsAny<string>())).Returns(new List<User> { user });

            var response = service.CreateUser(email, password);

            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(Resources.Messages.CreateUserError, response.Message);
        }

        [TestMethod()]
        public void RemoveUserWithExistingEmailTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RemoveUserWithNonExistingEmailTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateUserWithExistingEmailTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateUserWithNonExistingEmailTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void LoginWithValidDetailsTest()
        {
            Assert.Fail();
        }
        [TestMethod()]
        public void LoginWithInvalidDetailsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InitiateForgotPasswordWithExistingEmailTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InitiateForgotPasswordWithNonExistingEmailTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ForgotPasswordResetWithExistingKeyTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ForgotPasswordResetWithNonExistingKeyTest()
        {
            Assert.Fail();
        }
    }
}