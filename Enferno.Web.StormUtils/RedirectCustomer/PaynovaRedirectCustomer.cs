
using System.Web;
using Enferno.StormApiClient.Shopping;

namespace Enferno.Web.StormUtils
{
    public class PaynovaRedirectCustomer : AbstractRedirectCustomer
    {
        protected override void Redirect(HttpContext context, PaymentResponse response)
        {
            context.Response.Redirect(response.RedirectUrl, false);
            context.ApplicationInstance.CompleteRequest();
        }
    }
}
