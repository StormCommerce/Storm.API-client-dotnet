using Enferno.StormApiClient;
using Enferno.StormApiClient.Applications;
using Enferno.Web.StormUtils.InternalRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Enferno.Web.StormUtils.Test
{
    [TestClass]
    public class RepositoryTest : TestBase
    {       
        [TestMethod, TestCategory("UnitTest")]
        [Description("Tests creation of default AccessClient for the repository")]
        public void GetBatchTest1()
        {
            // Arrange
            var api = new AccessClient();
            var repository = new Repository(() => api);
            // Act
            var batch = repository.GetBatch();
            // Assert
            Assert.IsInstanceOfType(batch, typeof(AccessClient));
        }

        [TestMethod, TestCategory("UnitTest")]
        [Description("Tests creation of mock IAccessClient for the repository")]
        public void GetBatchTest2()
        {
            // Arrange
            var repository = new Repository(() => MockRepository.GenerateMock<IAccessClient>());
            // Act
            var batch = repository.GetBatch();
            // Assert
            Assert.IsNotInstanceOfType(batch, typeof(AccessClient));
        }

        [TestMethod, TestCategory("UnitTest")]
        [Description("Tests creation of mock IAccessClient for the repository, transient")]
        public void GetBatchTest3()
        {
            // Arrange
            var repository = new Repository(() => MockRepository.GenerateMock<IAccessClient>());
            // Act
            var batch1 = repository.GetBatch();
            var batch2 = repository.GetBatch();
            // Assert
            Assert.AreNotSame(batch1, batch2);
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GetApplicationTest()
        {
            // Arrange
            var repository = new Repository(() =>
            {
                var api = MockRepository.GenerateMock<IAccessClient>();
                var svc = MockRepository.GenerateMock<ApplicationService>();
                svc.Stub(x => x.GetApplication("sv-SE")).IgnoreArguments().Return(CreateDefaultApplication());
                api.Stub(x => x.ApplicationProxy).Return(svc);
                return api;
            });

            // Act
            var application = repository.GetApplication("sv-SE");

            // Assert
            Assert.AreEqual(1, application.Id);
        }
    }
}
