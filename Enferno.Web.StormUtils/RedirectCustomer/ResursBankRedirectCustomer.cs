
using System.Linq;
using Enferno.StormApiClient.Shopping;
using System.Web;

namespace Enferno.Web.StormUtils
{
    public class ResursBankRedirectCustomer : AbstractRedirectCustomer
    {
        protected override void Redirect(HttpContext context, PaymentResponse response)
        {
            StormContext.SessionItems["paymentcode"] = response.PaymentCode;
            if (response.RedirectParameters != null && response.RedirectParameters.Any(p => p.Name == "usecompanycard"))
            {
                StormContext.SessionItems["usecompanycard"] = "true";
            }
            context.Response.Redirect(response.RedirectUrl, false);
            context.ApplicationInstance.CompleteRequest();
        }
    }
}
