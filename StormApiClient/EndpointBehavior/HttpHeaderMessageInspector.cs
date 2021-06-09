using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Enferno.StormApiClient.EndpointBehavior
{
    public class HttpHeaderMessageInspector : IClientMessageInspector
    {
        private readonly Dictionary<string, string> _httpHeaders;

        public HttpHeaderMessageInspector(Dictionary<string, string> httpHeaders)
        {
            this._httpHeaders = httpHeaders;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState) { }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;

            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;

                foreach (var httpHeader in _httpHeaders)
                {
                    httpRequestMessage.Headers[httpHeader.Key] = httpHeader.Value;
                }
            }
            else
            {
                httpRequestMessage = new HttpRequestMessageProperty();

                foreach (var httpHeader in _httpHeaders)
                {
                    httpRequestMessage.Headers.Add(httpHeader.Key, httpHeader.Value);
                }
                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
            }
            return null;
        }
    }

  
}
