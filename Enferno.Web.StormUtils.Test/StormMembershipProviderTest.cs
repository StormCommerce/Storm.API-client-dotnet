
using System.Collections.Specialized;
using System.Web.Security;
using Enferno.StormApiClient.Expose;
using Enferno.Web.StormUtils.InternalRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Enferno.Web.StormUtils.Test
{
    [TestClass]
    public class StormMembershipProviderTest : TestBase
    {
        [TestMethod]
        public void InitializeTest1()
        {
            // Arrange
            var provider = new StormMembershipProvider();
            var config = new NameValueCollection { { "applicationName", "TestApplication" } };

            // Act
            provider.Initialize("Test", config);

            // Assert
            Assert.AreEqual("Test", provider.Name);
            Assert.AreEqual("TestApplication", provider.ApplicationName);
        }

        [TestMethod]
        public void ChangePasswordWrongPassword1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            customer.Account.Authorizations.Add(new IdValue { Id = 1, Value = "NormalUser" });
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByEmail("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);
            repository.Stub(x => x.Login("kalle.anka@ankeborg.se", "abc")).IgnoreArguments().Return(null);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormMembershipProvider(repository);

            // Act
            var success = provider.ChangePassword("kalle.anka@ankeborg.se", "abc", "def");

            // Assert
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void ValidateUserTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            customer.Account.Authorizations.Add(new IdValue { Id = 1, Value = "NormalUser" });
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByEmail("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);
            repository.Stub(x => x.Login("kalle.anka@ankeborg.se", "abc")).IgnoreArguments().Return(customer);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormMembershipProvider(repository);

            // Act
            var success = provider.ValidateUser("kalle.anka@ankeborg.se", "abc");

            // Assert
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void ValidateUserTest2()
        {
            // Arrange
            var provider = new StormMembershipProvider();

            // Act
            var success = provider.ValidateUser(null, "abc");

            // Assert
            Assert.IsFalse(success);
        }

        [TestMethod]
        public void ChangePassword1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            customer.Account.Authorizations.Add(new IdValue { Id = 1, Value = "NormalUser" });
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByEmail("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);
            repository.Stub(x => x.Login("kalle.anka@ankeborg.se", "abc")).IgnoreArguments().Return(customer);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormMembershipProvider(repository);

            // Act
            var success = provider.ChangePassword("kalle.anka@ankeborg.se", "abc", "def");

            // Assert
            Assert.IsTrue(success);
            repository.AssertWasCalled(x => x.ChangePassword("kalle.anka@ankeborg.se", "abc", "def", "def"), o => o.IgnoreArguments());
        }

        [TestMethod]
        public void ResetPassword1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByEmail("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);
            repository.Stub(x => x.Login("kalle.anka@ankeborg.se", "abc")).IgnoreArguments().Return(customer);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormMembershipProvider(repository);

            // Act
            provider.ResetPassword("kalle.anka@ankeborg.se", "answer");

            // Assert
            repository.AssertWasCalled(x => x.SendPasswordReminder("kalle.anka@ankeborg.se"), o => o.IgnoreArguments());
        }

        [TestMethod]
        public void GetUserTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            customer.Account.Authorizations.Add(new IdValue { Id = 1, Value = "NormalUser" });
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByLoginName("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormMembershipProvider(repository);
            InitializeProvider(provider);

            // Act
            var member = provider.GetUser("kalle.anka@ankeborg.se", true);

            // Assert
            Assert.IsNotNull(member);
            Assert.AreEqual("kalle.anka@ankeborg.se", member.UserName);
        }

        [TestMethod]
        public void GetUserTest2()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            customer.Account.Authorizations.Add(new IdValue { Id = 1, Value = "NormalUser" });
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByKey(customer.Key)).IgnoreArguments().Return(customer);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormMembershipProvider(repository);
            InitializeProvider(provider);

            // Act
            var member = provider.GetUser(customer.Key, true);

            // Assert
            Assert.IsNotNull(member);
            Assert.AreEqual("kalle.anka@ankeborg.se", member.UserName);
        }

        [TestMethod]
        public void GetUserNameByEmail1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            customer.Account.Authorizations.Add(new IdValue { Id = 1, Value = "NormalUser" });
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByEmail("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormMembershipProvider(repository);
            InitializeProvider(provider);

            // Act
            var name = provider.GetUserNameByEmail("kalle.anka@ankeborg.se");

            // Assert
            Assert.AreEqual("kalle.anka@ankeborg.se", name);
        }

        [TestMethod]
        public void CreateUserTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            customer.Account.Authorizations.Add(new IdValue { Id = 1, Value = "NormalUser" });
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByEmail("kalle.anka@ankeborg.se")).IgnoreArguments().Return(null);
            repository.Stub(x => x.CreateCustomer(customer)).IgnoreArguments().Return(customer);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormMembershipProvider(repository);
            InitializeProvider(provider);

            // Act
            MembershipCreateStatus status;
            var user = provider.CreateUser("kalle.anka@ankeborg.se", "abc", "kalle.anka@ankeborg.se", "question", "answer", true, null, out status);

            // Assert
            Assert.AreEqual(MembershipCreateStatus.Success, status);
            Assert.IsNotNull(user);
            Assert.AreEqual("kalle.anka@ankeborg.se", user.UserName);
        }

        [TestMethod]
        public void CreateUserWhenUserExistsTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            customer.Account.Authorizations.Add(new IdValue { Id = 1, Value = "NormalUser" });
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByLoginName("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormMembershipProvider(repository);
            InitializeProvider(provider);

            // Act
            MembershipCreateStatus status;
            provider.CreateUser("kalle.anka@ankeborg.se", "abc", "kalle.anka@ankeborg.se", "question", "answer", true, null, out status);

            // Assert
            Assert.AreEqual(MembershipCreateStatus.DuplicateUserName, status);
        }

        private static void InitializeProvider(StormMembershipProvider provider)
        {
            var config = new NameValueCollection { { "applicationName", "TestApplication" } };
            provider.Initialize("Test", config);
        }
    }
}
