
using System;
using System.Web;

namespace Enferno.Web.StormUtils
{
    public interface ICallbackHandler : IHttpHandler
    {
        event EventHandler DefaultRedirect;
        event EventHandler SuccessRedirect;
        event EventHandler FailRedirect;

        event NotificationEventHandler OnOrderCallbackNotification;

        string StatusMessage { get; set; }
    }

	public abstract class AbstractCallbackHandler : ICallbackHandler {
		public event EventHandler DefaultRedirect;
		public event EventHandler SuccessRedirect;
		public event EventHandler FailRedirect;
		public event NotificationEventHandler OnOrderCallbackNotification;

		public virtual void ProcessRequest(HttpContext context) {
			throw new NotImplementedException();
		}

		public string StatusMessage { get; set; }

		public bool IsReusable {
			get { return true; }
		}

		public void Default(HttpContext context) {
			if(DefaultRedirect != null)
				DefaultRedirect(this, new EventArgs());
			else
				RedirectToDefault(context);
		}

		public void Success(HttpContext context) {
		    OnOrderCallbackNotification?.Invoke(this, new NotificationEventArgs(StormContext.BasketId.Value));
		    StormContext.ConfirmedBasketId = StormContext.BasketId;
			StormContext.BasketId = null;
			if(SuccessRedirect != null)
				SuccessRedirect(this, new EventArgs());
			else
				RedirectToConfirmed(context);
		}

		public void Fail(HttpContext context) {
			if(FailRedirect != null)
				FailRedirect(this, new EventArgs());
			else
				RedirectToCheckout(context);
		}

		protected virtual void RedirectToDefault(HttpContext context) {
			new Link(StormContext.Configuration.DefaultUrl).Redirect();
		}

		protected virtual void RedirectToConfirmed(HttpContext context) {
			new Link(StormContext.Configuration.ConfirmedUrl).Redirect();
		}

		protected virtual void RedirectToCheckout(HttpContext context) {
			new Link(StormContext.Configuration.CheckoutUrl).Set("paymentstatus", this.StatusMessage).Redirect();
		}
	}
}
