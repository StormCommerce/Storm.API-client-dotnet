
using System.Web;
using Enferno.Web.StormUtils.InternalRepository;
using Expose = Enferno.StormApiClient.Expose;

namespace Enferno.Web.StormUtils
{
    public class ResursBankCallbackHandler : AbstractCallbackHandler
    {
        private readonly IRepository repository;
        private bool useCompanyCard;

        public ResursBankCallbackHandler()
        {
            repository = new Repository();
        }

        /// <summary>
        /// For test only
        /// </summary>
        /// <param name="repository">Repository mock</param>
        public ResursBankCallbackHandler(IRepository repository)
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
            var paymentParameters = GetParameters();

            try
            {
                var response = useCompanyCard ? repository.PaymentCallback(paymentParameters) : repository.PaymentCallback2(paymentParameters);

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
        
        private Expose.NameValues GetParameters()
        {
            var parameters = new Expose.NameValues();
            if (StormContext.SessionItems["paymentcode"] != null)
            {
                parameters.Add(new Expose.NameValue { Name = "PaymentService", Value = "ResursBank" });
                parameters.Add(new Expose.NameValue { Name = "paymentcode", Value = StormContext.SessionItems["paymentcode"] });

                useCompanyCard = false;
                if (StormContext.SessionItems["usecompanycard"] != null)
                {
                    useCompanyCard = true;
                    StormContext.SessionItems["usecompanycard"] = null;
                }

                StormContext.SessionItems["paymentcode"] = null;
            }
           
            return parameters;
        }
    }
}
