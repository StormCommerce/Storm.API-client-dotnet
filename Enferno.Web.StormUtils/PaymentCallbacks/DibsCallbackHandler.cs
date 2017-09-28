
using System.Web;
using Enferno.Web.StormUtils.InternalRepository;
using Expose = Enferno.StormApiClient.Expose;
using Enferno.Public.Logging;

namespace Enferno.Web.StormUtils
{
    public class DibsCallbackHandler : AbstractCallbackHandler
    {
        private readonly IRepository repository;

        public DibsCallbackHandler()
        {
            repository = new Repository();
        }

        /// <summary>
        /// For test only
        /// </summary>
        /// <param name="repository">Repository mock</param>
        public DibsCallbackHandler(IRepository repository)
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

            var basket = repository.GetBasket(StormContext.BasketId.Value);
            var paymentParameters = GetParameters(context);

            if (IsValid(paymentParameters))
            {
                try
                {
                    var response = repository.PaymentCallback2(paymentParameters);
                    StatusMessage = response.StatusDescription;
                    if (response.Status == "OK")
                    {
                        Success(context);
                    }
                    else
                    {
                        basket = repository.PaymentCancel(basket);
                        Fail(context);
                    }
                }
                catch (System.ServiceModel.FaultException<Expose.ErrorMessage_v2.ErrorMessage> ex)
                {
                    if (ex.Message == "Bad Request")
                    {
                        StatusMessage = ex.Detail.Message;
                        repository.PaymentCancel(basket);
                        Fail(context);
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            else //if (status etc do not exist)
            {
                Log.LogEntry.Categories(CategoryFlags.Debug | CategoryFlags.Alert).Message("Failed to complete Callback with parameters: {0}", WriteParameters(paymentParameters)).WriteWarning();
                repository.PaymentCancel(basket);
                Fail(context);
            }
        }

        private bool IsValid(Expose.NameValues paymentParameters)
        {
            return (paymentParameters.Exists(p => p.Name == "status") || paymentParameters.Exists(p => p.Name == "statuscode")) &&
                paymentParameters.Exists(p => p.Name == "s_paymentCode") &&
                paymentParameters.Exists(p => p.Name == "s_applicationKey");
        }

        private static Expose.NameValues GetParameters(HttpContext context)
        {
            var parameters = new Expose.NameValues();

            foreach (string key in context.Request.Form.Keys)
            {
                parameters.Add(new Expose.NameValue { Name = key, Value = context.Request.Form[key] });
            }

            foreach (var key in context.Request.QueryString.AllKeys)
            {
                parameters.Add(new Expose.NameValue { Name = key, Value = context.Request.QueryString[key] });
            }

            Log.LogEntry.Categories(CategoryFlags.Debug).Message("Callback parameters: {0}", WriteParameters(parameters)).WriteVerbose();
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
