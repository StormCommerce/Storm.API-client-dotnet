
using System;
using System.Web;
using Enferno.StormApiClient.Shopping;
using Enferno.StormApiClient;

namespace Enferno.Web.StormUtils
{
    public class NotificationEventArgs : EventArgs
    {
        public readonly int BasketId;
        
        public NotificationEventArgs(int basketId)
        {
            BasketId = basketId;    
        }
    }

    public delegate void NotificationEventHandler(object sender, NotificationEventArgs args);

    public interface IRedirectCustomer
    {
        void RedirectCustomer(IAccessClient client, Basket currentBasket, HttpContext context, PaymentResponse response, string confirmedUrl);

        event NotificationEventHandler OnOrderConfirmedNotification;
    }

    public abstract class AbstractRedirectCustomer : IRedirectCustomer
    {
        
        public event NotificationEventHandler OnOrderConfirmedNotification;

        public void RedirectCustomer(IAccessClient client, Basket currentBasket, HttpContext context, PaymentResponse response, string confirmedUrl)
        {
            if (response.Status == "OK")
            {
                if (!string.IsNullOrEmpty(response.RedirectUrl))
                {
                    Redirect(context, response);
                }
                else
                {
                    OnOrderConfirmedNotification?.Invoke(this, new NotificationEventArgs(StormContext.BasketId.Value));
                    StormContext.ConfirmedBasketId = StormContext.BasketId;
                    StormContext.BasketId = null;
                    context.Response.Redirect(confirmedUrl, false);
                    context.ApplicationInstance.CompleteRequest();
                }
            }
            else
            {
                client.ShoppingProxy.PaymentCancel(currentBasket);
                throw new ApplicationException($"Fel i betalningen: {response.StatusDescription}");
            }

        }

        protected abstract void Redirect(HttpContext context, PaymentResponse response);
    }
}
