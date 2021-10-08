using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Enferno.StormApiClient.Test.Mocks
{
    public class HttpMessageHandlerMock : HttpMessageHandler
    {
        private HttpStatusCode statusCode;
        private HttpContent content;

        public void SetupResponse(HttpStatusCode statusCode, string content)
        {
            this.statusCode = statusCode;
            this.content = new StringContent(content);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode) { Content = content });
        }
    }
}
