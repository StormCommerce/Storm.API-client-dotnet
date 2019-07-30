using System.Linq;
using Enferno.StormApiClient.Shopping;
using System.Web;

namespace Enferno.Web.StormUtils
{
    public class DibsAndResursBankRedirectCustomer : AbstractRedirectCustomer
    {
        protected override void Redirect(HttpContext context, PaymentResponse response)
        {
            if (response.RedirectParameters != null && response.RedirectParameters.FirstOrDefault(p => p.Name == "s_paymentCode" || p.Name == "s_quotationId") != null)
            {
                StormContext.SessionItems["paymentcode"] = null;

                context.Response.Clear();
                context.Response.Write("<html><head></head><body>");
                context.Response.Write(string.Format(
                    @"<form name='newForm' target='_parent' method=post action='{0}'>", response.RedirectUrl));

                foreach (var item in response.RedirectParameters)
                {
                    context.Response.Write(string.Format(@"<input type=hidden name='{0}' value='{1}'>", item.Name,
                        item.Value));
                }

                context.Response.Write("</form>");
                context.Response.Write("<SCRIPT LANGUAGE='JavaScript'>document.forms[0].submit();</SCRIPT>");
                context.Response.Write("</body></html>");
                context.ApplicationInstance.CompleteRequest();
            }
            else
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
}
