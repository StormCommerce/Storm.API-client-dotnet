
using Enferno.StormApiClient;

namespace Enferno.Web.StormUtils
{
    public static class StormExtensions
    {
        private static readonly object syncRoot = new object();

        public static IAccessClient GetAccessClient(this System.Web.UI.Page page)
        {
            var client = page.Items["EnfernoStormApiClient"] as IAccessClient;
            if (client != null) return client;
            
            lock (syncRoot)
            {
                client = page.Items["EnfernoStormApiClient"] as IAccessClient;
                if (client != null) return client;

                client = new AccessClient("AccessClient");
                page.Unload += (o, e) => client.Dispose();
                page.PreRender += (o, e) => client.ProcessRequests(page);
                page.Items["EnfernoStormApiClient"] = client;
            }
            return client;
        }
    }
}
