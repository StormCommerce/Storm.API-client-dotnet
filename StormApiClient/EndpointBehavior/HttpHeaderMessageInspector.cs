using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, string> _httpHeaders;

        public HttpHeaderMessageInspector(ConcurrentDictionary<string, string> httpHeaders)
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
                AddHeaders(httpRequestMessage);
            }
            else
            {
                httpRequestMessage = new HttpRequestMessageProperty();
                AddHeaders(httpRequestMessage);
                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
            }
            return null;
        }

        private void AddHeaders(HttpRequestMessageProperty httpRequestMessage)
        {
            var keys = _httpHeaders.Keys;

            foreach (var key in keys)
            {
                if (_httpHeaders.TryGetValue(key, out string value))
                {
                   httpRequestMessage.Headers[key] = value;
                }

               
            }
        }
    }

  
}
