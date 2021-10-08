using Enferno.StormApiClient.OAuth2;
using Enferno.StormApiClient.Test.Builders;
using Enferno.StormApiClient.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Enferno.StormApiClient.Test
{
    [TestClass]
    public class OAuth2TokenResolverTests
    {
        private const string ValidResponse = @"{""access_token"":""eyJhbGciOiJSUzI1NiIsImtpZCI6IkU2MTMzN0U2Qzk3Rjc4ODcyMkUxQTAyOTc2MjgxRTg5RjFFRDcyODIiLCJ0eXAiOiJhdCtqd3QiLCJ4NXQiOiI1aE0zNXNsX2VJY2k0YUFwZGlnZWlmSHRjb0kifQ.eyJuYmYiOjE2MzM2MTI4MTIsImV4cCI6MTYzMzYxNjQxMiwiaXNzIjoiaHR0cHM6Ly9pZGVudGl0eS5zdG9ybS5pby8xLjAvIiwiYXVkIjoibGFiIiwiY2xpZW50X2lkIjoiMDIxYjllNTctZWEyMi00MTcxLWEzMWEtMWNjODg3MjQ0M2U5Iiwic2NvcGUiOlsibGFiIl19.el6mMn8Zt89vGkvSHPlD7MZl30JcuIRSe6y8zQEthHTtAe7dOy7fQn60e0OUklvmXbCOKXMXmQUQ7j_tQqoI4iYifQC7ypw6kDrNHPaF4Ib-EzzJlgZGWZ900-wngA9Fa_Rk9hxURgMA7okmEtfH6AUiSSjmyIACR3V0PuLhMuComuF9ZmebfJ107DpK4LnAGYt3QmINWZNGe_SVDvhbXiWgblVk8eDsqkTK-_a3q01FXenmpVa8yd4Sf8JZuMmcmHMB1LMrxZiZn8HPiPu7e4Zz6ytIrHDqCWZuli_u_kToLbYtRqZns-50wOmANu4tVW6mw29tS7JlbjfkZWUXxAlHjE6cDdihkPFltcm8aVr6iy3XJ79StT0LCqmKlS88FhEqk0xEiPaIsiKC_aE_V73m0NNjVFuOmOiryHdpyumKI5b25NK2fOkLQGRA_WWGUF_hkU45QrNSlNakRJWpYGbitx5R4KzFdwo0a5i6uib7nHO5EKB-pc7hIm6w3TyybXA4ZUnIuMLNk8DXwJnAE7ETV0Kmf2vPaxfeKjxvGu3dBONLs-F8gQpbuqFSWqsezc2fvnkjM7Ig4OLXSRytYNocLff_pe-8GNb4d1bZhJ-JP2QAT-AGP-w5aydJBeJzwBz3YseEFCmVBWaK8tMbrIDWWhI9Bu61mor7nXDanxw"",""expires_in"":3600,""token_type"":""Bearer"",""scope"":""lab""}";
        private const string ErrorResponse = @"{""error"":""invalid_scope""}";

        private OAuth2TokenResolver sut;
        private HttpMessageHandlerMock httpMessageHandlerMock;

        [TestInitialize]
        public void Initialize()
        {
            httpMessageHandlerMock = new HttpMessageHandlerMock();
            HttpClient httpClient = new HttpClient(httpMessageHandlerMock);

            sut = new OAuth2TokenResolver(httpClient);
        }

        [TestMethod]
        public async Task GetToken_ValidResponse_ReturnsToken()
        {
            httpMessageHandlerMock.SetupResponse(HttpStatusCode.OK, ValidResponse);

            var token = await sut.GetToken(Builder.OAuth2Credentials);

            Assert.IsNotNull(token);
            Assert.IsFalse(string.IsNullOrWhiteSpace(token.AccessToken));
        }


        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task GetToken_ErrorResponse_ThrowsHttpException()
        {
            httpMessageHandlerMock.SetupResponse(HttpStatusCode.BadRequest, ErrorResponse);

            _ = await sut.GetToken(Builder.OAuth2Credentials);
        }
    }
}
