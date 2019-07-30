
using System;
using System.Text;
using System.Web;
using Enferno.Web.StormUtils.InternalRepository;
using Expose = Enferno.StormApiClient.Expose;
using Enferno.Public.Logging;

namespace Enferno.Web.StormUtils
{
    public class DibsAndResursBankCallbackHandler : AbstractCallbackHandler
    {
        private readonly IRepository repository;
        private bool useCompanyCard;

        public DibsAndResursBankCallbackHandler()
        {
            repository = new Repository();
        }

        /// <summary>
        /// For test only
        /// </summary>
        /// <param name="repository">Repository mock</param>
        public DibsAndResursBankCallbackHandler(IRepository repository)
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
                    var response = useCompanyCard ? repository.PaymentCallback(paymentParameters) : repository.PaymentCallback2(paymentParameters);
                    StatusMessage = response.StatusDescription;
                    if (response.Status == "OK")
                    {
                        Success(context);
                    }
                    else
                    {
                        Log.LogEntry.Categories(CategoryFlags.Debug | CategoryFlags.Alert).Message("Status != OK, Failed to complete Callback with parameters: {0}", WriteParameters(paymentParameters)).WriteWarning();
                        basket = repository.PaymentCancel(basket);
                        Fail(context);
                    }
                }
                catch (System.ServiceModel.FaultException<Expose.ErrorMessage_v2.ErrorMessage> ex)
                {
                    if (ex.Message == "Bad Request")
                    {
                        StatusMessage = ex.Detail.Message;
                        Log.LogEntry.Categories(CategoryFlags.Debug | CategoryFlags.Alert)
                            .Message("Exception, Bad Request, failed to complete Callback with parameters: {0}", WriteParameters(paymentParameters))
                            .Message("Exception: {0}", ex)
                            .WriteWarning();
                        repository.PaymentCancel(basket);
                        Fail(context);
                    }
                    else
                    {
                        Log.LogEntry.Categories(CategoryFlags.Debug | CategoryFlags.Alert)
                            .Message("Exception, Failed to complete Callback with parameters: {0}", WriteParameters(paymentParameters))
                            .Message("Exception: {0}", ex)
                            .WriteWarning();
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
            return ((paymentParameters.Exists(p => p.Name == "status") || paymentParameters.Exists(p => p.Name == "statuscode")) &&
                paymentParameters.Exists(p => p.Name == "s_paymentCode" || p.Name == "s_quotationId") &&
                paymentParameters.Exists(p => p.Name == "s_applicationKey")) || (paymentParameters.Exists(p => p.Name == "PaymentService") &&
                paymentParameters.Exists(p => p.Name == "paymentcode"));
        }

        private Expose.NameValues GetParameters(HttpContext context)
        {
            var parameters = new Expose.NameValues();
            Log.LogEntry.Categories(CategoryFlags.Debug).Message("Callback request: {0}", context.Request.PhysicalPath).WriteVerbose();
            Log.LogEntry.Categories(CategoryFlags.Debug).Message("Callback request, raw: {0}", context.Request.RawUrl).WriteVerbose();
            
            foreach (string key in context.Request.Form.Keys)
            {
                parameters.Add(new Expose.NameValue { Name = key, Value = context.Request.Form[key] });
            }

            foreach (var key in context.Request.QueryString.AllKeys)
            {
                parameters.Add(new Expose.NameValue { Name = key, Value = context.Request.QueryString[key] });
            }

            if (!string.IsNullOrEmpty(StormContext.SessionItems["paymentcode"]))
            {
                parameters.Add(new Expose.NameValue { Name = "PaymentService", Value = "ResursBank" });
                parameters.Add(new Expose.NameValue { Name = "paymentcode", Value = StormContext.SessionItems["paymentcode"] });

                useCompanyCard = false;
                if (!string.IsNullOrEmpty(StormContext.SessionItems["usecompanycard"]))
                {
                    useCompanyCard = true;
                    StormContext.SessionItems["usecompanycard"] = null;
                }

                StormContext.SessionItems["paymentcode"] = null;
            }

            Log.LogEntry.Categories(CategoryFlags.Debug).Message("Callback parameters: {0}", WriteParameters(parameters)).WriteVerbose();
            return parameters;
        }

        private static string WriteParameters(Expose.NameValues parameters)
        {
            var str = new StringBuilder();
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
