using System;
using System.Collections.Generic;
using System.Web;
using System.IO;


namespace Enferno.Web.StormUtils
{
    /* 	<system.webServer>
        <modules runAllManagedModulesForAllRequests="true">
            <add name="StormModule" type="Enferno.Web.StormUtils.StormModule, Enferno.Web.StormUtils" preCondition="" />
        </modules>
     */
    public class StormModule : IHttpModule
    {
        public string ModuleName => "StormModule";

        public void Init(HttpApplication application)
        {
            application.BeginRequest += BeginRequest;
            application.EndRequest += EndRequest;
        }

        public void BeginRequest(object source, EventArgs e)
        {
            if (!IsRealPage) return;
            StormContext.LoadCookie();
            if (HttpContext.Current != null)
            {
                SetReferUrl();
                SetReferId();
            }
            if (StormContext.IsInitialRequest && !StormContext.RememberMe) StormContext.LogoutUser();
        }

        private static void SetReferUrl()
        {
            var referUrl = HttpContext.Current.Request.UrlReferrer;
            if (!string.IsNullOrEmpty(referUrl?.Host) && referUrl.Host != HttpContext.Current.Request.Url.Host)
            {
                StormContext.ReferUrl = referUrl.Host;
            }
        }

        private static void SetReferId()
        {
            var rid = HttpContext.Current.Request.QueryString["referid"];
            if (string.IsNullOrWhiteSpace(rid)) return;
            int i;
            if (int.TryParse(rid, out i)) StormContext.ReferId = i;
        }

        public void EndRequest(object source, EventArgs e)
        {
            if (!IsRealPage) return;
            if (StormContext.IsInitialRequest && HttpContext.Current.Response.RedirectLocation == null)
            {
                StormContext.IsInitialRequest = false;
                StormContext.Refresh();
            }
            StormContext.SaveCookie();
        }

        public bool IsRealPage
        {
            get
            {
                if (HttpContext.Current == null) return false;
                try
                {
                    return new List<string> {".aspx", ".ashx", ""}.Contains(Path.GetExtension(HttpContext.Current.Request.Path));
                }
                catch (Exception)
                {
                    // Not a valid path, could be a JavaSript url or some unknown type. Don't treat it as a real page.
                }
                return false;
            }
        }

        public void Dispose()
        {
        }
    }
}
