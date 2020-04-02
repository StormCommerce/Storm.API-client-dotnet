
using System.Web;
using Enferno.Web.StormUtils.InternalRepository;
using Expose = Enferno.StormApiClient.Expose;

namespace Enferno.Web.StormUtils
{
    public class PayexCallbackHandler : AbstractCallbackHandler
    {
        private readonly IRepository repository;

        public PayexCallbackHandler()
        {
            repository = new Repository();
        }

        /// <summary>
        /// For test only
        /// </summary>
        /// <param name="repository">Repository mock</param>
        public PayexCallbackHandler(IRepository repository)
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

            var orderRef = context.Request["orderRef"];
            var basket = repository.GetBasket(StormContext.BasketId.Value);

            if (!string.IsNullOrEmpty(orderRef))
            {
                try
                {
                    var paymentParameters = GetParameters(context);
                    var response = repository.PaymentCallback(paymentParameters);
                    this.StatusMessage = response.StatusDescription;
                    if (response.Status == "OK")
                    {
                        Success(context);
                    }
                    else
                    {
                        repository.PaymentCancel(basket);
                        Fail(context);
                    }
                }
                catch (System.ServiceModel.FaultException<Expose.ErrorMessage_v2.ErrorMessage> ex)
                {
                    if (ex.Message == "Bad Request")
                    {
                        this.StatusMessage = ex.Detail.Message;
                        Fail(context);
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            else //if (status == "failed")
            {
                repository.PaymentCancel(basket);
                Fail(context);
            }
        }

        private static Expose.NameValues GetParameters(HttpContext context)
        {
            var parameters = new Expose.NameValues();

            foreach (string key in context.Request.QueryString.Keys)
            {
                parameters.Add(new Expose.NameValue { Name = key, Value = context.Request.QueryString[key] });
            }

            foreach (string key in context.Request.Form.Keys)
            {
                parameters.Add(new Expose.NameValue { Name = key, Value = context.Request.Form[key] });
            }

            AddParameterIfNotExists(parameters, "PaymentService", "Payex");

            return parameters;
        }
    }
}
