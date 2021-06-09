using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Enferno.StormApiClient.EndpointBehavior
{
    public class HttpHeadersEndpointBehavior : IEndpointBehavior
    {
        public Dictionary<string, string> _httpHeaders { get; set; }

        public HttpHeadersEndpointBehavior(Dictionary<string, string> httpHeaders)
        {
            this._httpHeaders = httpHeaders;
        }

        public void Validate(ServiceEndpoint endpoint)
        {
          
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            var inspector = new HttpHeaderMessageInspector(this._httpHeaders);

            clientRuntime.MessageInspectors.Add(inspector);
        }
    }
}
