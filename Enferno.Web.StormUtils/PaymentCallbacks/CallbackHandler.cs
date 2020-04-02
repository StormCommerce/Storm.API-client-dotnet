using System.Diagnostics;
using System.Linq;
using System.Web;
using Enferno.Web.StormUtils.InternalRepository;
using Expose = Enferno.StormApiClient.Expose;
using Enferno.Public.Logging;

namespace Enferno.Web.StormUtils
{
    public class CallbackHandler : AbstractCallbackHandler
    {
        private readonly IRepository repository;
        private bool completeAfterCallback;

        public CallbackHandler()
        {
            repository = new Repository();
        }

        public CallbackHandler(IRepository repository)
        {
            this.repository = repository;
        }

        public override void ProcessRequest(HttpContext context)
        {
            if (!StormContext.BasketId.HasValue)
            {
                Default(context);
                return;
            }

            var checkout = repository.GetCheckout(StormContext.BasketId.Value);
            var paymentParameters = GetParameters(context, checkout?.PaymentMethods?.FirstOrDefault(p => p.IsSelected)?.Service?.Id);

            try
            {
                var response = completeAfterCallback ? repository.PaymentCallback(paymentParameters) : repository.PaymentCallback2(paymentParameters);
                StatusMessage = response.StatusDescription;
                if (response.Status == "OK")
                {
                    Success(context);
                }
                else
                {
                    repository.PaymentCancel(checkout.Basket);
                    Fail(context);
                }
            }
            catch (System.ServiceModel.FaultException<Expose.ErrorMessage_v2.ErrorMessage> ex)
            {
                if (ex.Message == "Bad Request")
                {
                    StatusMessage = ex.Detail.Message;
                    repository.PaymentCancel(checkout.Basket);
                    Fail(context);
                }
                else
                {
                    throw;
                }
            }
        }

        private Expose.NameValues GetParameters(HttpContext context, int? paymentServiceId)
        {
            if(paymentServiceId == null) return new Expose.NameValues();

            switch (paymentServiceId.Value)
            {
                case 1:
                    completeAfterCallback = true;
                    return GetFormParameters(context, "Payex");
                case 4:
                    return GetFormParameters(context, "Dibs");
                case 9:
                    return GetFormParameters(context, "Paynova");
                case 11:
                    return GetResursBankParameters();
                case 15:
                    return GetFormParameters(context, "Verifone");
                case 16:
                    return GetFormParameters(context, "Adyen");
                case 17:
                    return GetFormParameters(context, "DLL");
            }

            return new Expose.NameValues();
        }

        private static Expose.NameValues GetFormParameters(HttpContext context, string paymentService)
        { 
            var parameters = new Expose.NameValues();

            foreach (string key in context.Request.Form.Keys)
            {
                parameters.Add(new Expose.NameValue { Name = key, Value = context.Request.Form[key] });
                Debug.WriteLine(key + ": " + context.Request.Form[key]);
            }

            foreach (var key in context.Request.QueryString.AllKeys)
            {
                parameters.Add(new Expose.NameValue { Name = key, Value = context.Request.QueryString[key] });
                Debug.WriteLine(key + ": " + context.Request.QueryString[key]);
            }

            AddParameterIfNotExists(parameters, "PaymentService", paymentService);

            Log.LogEntry.Categories(CategoryFlags.Debug).Message("Callback parameters: {0}", WriteParameters(parameters)).WriteVerbose();
            return parameters;
        }

        private Expose.NameValues GetResursBankParameters()
        {
            var parameters = new Expose.NameValues();
            if (StormContext.SessionItems["paymentcode"] != null)
            {
                AddParameterIfNotExists(parameters, "PaymentService", "ResursBank");
                parameters.Add(new Expose.NameValue { Name = "paymentcode", Value = StormContext.SessionItems["paymentcode"] });

                completeAfterCallback = false;
                if (!string.IsNullOrEmpty(StormContext.SessionItems["usecompanycard"]))
                {
                    completeAfterCallback = true;
                    StormContext.SessionItems["usecompanycard"] = null;
                }

                StormContext.SessionItems["paymentcode"] = null;
            }

            return parameters;
        }

        private static string WriteParameters(Expose.NameValues parameters)
        {
            var str = new System.Text.StringBuilder();
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var item in parameters)
                {
                    str.AppendFormat("{0}={1}, ", item.Name, item.Value);
                }
            }
            return str.ToString().TrimEnd(',', ' ');
        }
    }
}
