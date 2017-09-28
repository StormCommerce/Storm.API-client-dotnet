using System;
using System.Collections.Specialized;
using Enferno.StormApiClient.Expose;
using Enferno.Web.StormUtils.InternalRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Enferno.Web.StormUtils.Test
{
    [TestClass]
    public class StormRoleProviderTest : TestBase
    {
        [TestMethod]
        public void InitializeTest1()
        {
            // Arrange
            var provider = new StormRoleProvider();
            var config = new NameValueCollection {{"applicationName", "TestApplication"}};

            // Act
            provider.Initialize("Test", config);

            // Assert
            Assert.AreEqual("Test", provider.Name);
            Assert.AreEqual("TestApplication", provider.ApplicationName);
        }

        [TestMethod]
        public void InitializeWithDefaultTest1()
        {
            // Arrange
            var provider = new StormRoleProvider();
            var config = new NameValueCollection { { "applicationName", "TestApplication" } };

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            provider.Initialize(null, config);

            // Assert
            Assert.AreEqual("StormRoleProvider", provider.Name);
            Assert.AreEqual("TestApplication", provider.ApplicationName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InitializeWithNoConfigTest()
        {
            // Arrange
            var provider = new StormRoleProvider();

            // Act
            // ReSharper disable once AssignNullToNotNullAttribute
            provider.Initialize("Test", null);

            // Assert
            Assert.Fail("Should not get here");
        }

        [TestMethod]
        public void GetAllRolesTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();         
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormRoleProvider(repository);

            // Act
            var roles = provider.GetAllRoles();

            // Assert
            Assert.AreEqual(1, roles.Length);
            Assert.AreEqual("DefaultAuthorization", roles[0]);
        }

        [TestMethod]
        public void GetRolesForUserTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            customer.Account.Authorizations.Add(new IdValue { Id = 1, Value = "NormalUser"});
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByEmail("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormRoleProvider(repository);

            // Act
            var roles = provider.GetRolesForUser("kalle.anka@ankeborg.se");

            // Assert
            Assert.AreEqual(1, roles.Length);
            Assert.AreEqual("NormalUser", roles[0]);
        }

        [TestMethod]
        public void IsUserInRoleTest1()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var customer = CreateDefaultCustomer();
            customer.Account.Authorizations.Add(new IdValue {Id = 1, Value = "NormalUser"});
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);
            repository.Stub(x => x.GetCustomerByEmail("kalle.anka@ankeborg.se")).IgnoreArguments().Return(customer);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormRoleProvider(repository);

            // Act
            var isInRole = provider.IsUserInRole("kalle.anka@ankeborg.se", "NormalUser");

            // Assert
            Assert.IsTrue(isInRole);
        }

        [TestMethod]
        public void RoleExistsTest()
        {
            // Arrange
            var application = CreateDefaultApplication();
            var repository = MockRepository.GenerateMock<IRepository>();

            repository.Stub(x => x.GetApplication()).IgnoreArguments().Return(application);

            var httpContextWrapper = CreateEmptyHttpContextMock();
            var ctx = new StormContext(repository, httpContextWrapper);
            StormContext.SetInstance(ctx);

            var provider = new StormRoleProvider(repository);

            // Act
            var roleExists = provider.RoleExists("DefaultAuthorization");

            // Assert
            Assert.IsTrue(roleExists);
        }
    }
}
