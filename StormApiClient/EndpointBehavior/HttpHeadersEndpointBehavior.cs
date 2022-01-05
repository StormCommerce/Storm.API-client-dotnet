﻿using System;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<string, string> _httpHeaders;

        public HttpHeadersEndpointBehavior()
        {
            this._httpHeaders = new ConcurrentDictionary<string, string>();
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

        public void SetHeader(string key, string value)
        {
            _httpHeaders.AddOrUpdate(key, value,(k,oldValue)=>value);

        }
    }
}
