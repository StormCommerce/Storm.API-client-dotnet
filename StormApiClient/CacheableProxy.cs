using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using System.ServiceModel;
using Enferno.Public.Caching;
using Enferno.Public.Logging;

namespace Enferno.StormApiClient
{
    public sealed class CacheableProxy<T, TS> : RealProxy where T : ClientBase<TS>, TS, new() where TS : class
    {
        private readonly ICacheManager cacheManager;
        private readonly string cacheName;

        private readonly T instance;

        public static TS CreateProxy(ICacheManager cacheManager, string cacheName, T instance)
        {
            if (instance != null)
            {
                var ret = (TS)new CacheableProxy<T, TS>(cacheManager, cacheName, instance, typeof(TS)).GetTransparentProxy();
                return ret;
            }
            return default(TS);
        }

        private CacheableProxy(ICacheManager cacheManager, string cacheName, T mo, Type type) : base(type)
        {
            this.cacheManager = cacheManager;
            this.cacheName = cacheName;
            instance = mo;
        }

        public override IMessage Invoke(IMessage msg)
        {
            var mc = new MethodCallMessageWrapper((IMethodCallMessage)msg);
            var method = mc.MethodBase as MethodInfo;
            if(method == null) throw new NullReferenceException("Method is null");

            var args = ((method.Attributes & MethodAttributes.SpecialName) == MethodAttributes.SpecialName) ? mc.InArgs : mc.Args;
            object res;

            var argumentkey = cacheManager.GetArgumentKey(cacheName, method, args);
            var key = argumentkey ?? cacheManager.GetKey(method.Name, ApplicationArgs(args));
            if (IsCached(key, out res)) return CreateReturnMessage(mc, res);
            using(cacheManager.AcquireLock(cacheName, key))
            {
                if (IsCached(key, out res)) return CreateReturnMessage(mc, res);
                res = InvokeMethod(method, args);
                AddToCache(key, res);
            }

            return CreateReturnMessage(mc, res);
        }

        private object[] ApplicationArgs(object[] args)
        {
            if (instance.ClientCredentials?.ClientCertificate.Certificate == null) return args;
            
            var thumbprint = instance.ClientCredentials.ClientCertificate.Certificate.Thumbprint;
            return new object[] { thumbprint }.Union(args).ToArray();
        }

        private static ReturnMessage CreateReturnMessage(MethodCallMessageWrapper mc, object res)
        {
            if (mc == null) throw new ArgumentNullException("mc");
            return mc.Args == null ?
                new ReturnMessage(res, mc.Args, 0, mc.LogicalCallContext, mc) : 
                new ReturnMessage(res, mc.Args, mc.Args.Length, mc.LogicalCallContext, mc);
        }

        private object InvokeMethod(MethodInfo method, object[] args)
        {
            DebugLogMethodCall(method, args);

            try
            {
                return method.Invoke(instance, args);
            }
            catch (TargetInvocationException ex)
            {
                Log.LogEntry.Categories(AccessClient.LogCategory).Categories(CategoryFlags.Alert)
                    .Property("Certificate", instance.ClientCredentials?.ClientCertificate.Certificate?.ToString())
                    .Message("Failed CacheableProxy.InvokeMethod.").Exceptions(ex).WriteError();

                throw ex.InnerException;
            }
        }

        private static void DebugLogMethodCall(MethodInfo method, object[] args)
        {
            var entry = Log.LogEntry.Categories(AccessClient.LogCategory).Categories(CategoryFlags.Debug);
            entry.Message("Call to {0}.{1}", method.DeclaringType != null ? method.DeclaringType.Name : "-unknown-", method.Name);
            if (args != null)
            {
                var parameters = method.GetParameters();
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    var parameter = i < parameters.Length ? parameters[i] : null;
                    entry.Property(parameter != null ? parameter.Name : "arg" + i.ToString(CultureInfo.InvariantCulture), arg);
                }
            }
            entry.WriteVerbose();
        }

        private bool IsCached(string key, out object response)
        {
            response = default(object);
            if (!cacheManager.HasConfiguration(cacheName)) return false;
            return cacheManager.TryGet(cacheName, key, out response);
        }

        private void AddToCache(string key, object data)
        {
            if (!cacheManager.HasConfiguration(cacheName)) return;
            cacheManager.Add(cacheName, key, data);
        }
    }
}
