using System.Diagnostics;
using Enferno.StormApiClient.Shopping;
using System.Web;

namespace Enferno.Web.StormUtils
{
    public class VerifoneRedirectCustomer : AbstractRedirectCustomer
    {
        protected override void Redirect(HttpContext context, PaymentResponse response)
        {
            context.Response.Clear();
            context.Response.Write("<html><head></head><body>");
            context.Response.Write(string.Format(@"<form name='newForm' target='_parent' method=post action='{0}'>", response.RedirectUrl));

            foreach (var item in response.RedirectParameters)
            {
                context.Response.Write(string.Format(@"<input type=hidden name='{0}' value='{1}'>", item.Name, item.Value));

                Debug.WriteLine(item.Name + ": " + item.Value);
            }

            context.Response.Write("</form>");
            context.Response.Write("<SCRIPT LANGUAGE='JavaScript'>document.forms[0].submit();</SCRIPT>");
            context.Response.Write("</body></html>");
            context.Response.End();
        }
    }
}
