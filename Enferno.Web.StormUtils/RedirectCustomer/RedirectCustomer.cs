﻿using System;
using System.Diagnostics;
using System.Linq;
using Enferno.StormApiClient.Shopping;
using System.Web;

namespace Enferno.Web.StormUtils
{
    public class RedirectCustomer : AbstractRedirectCustomer
    {
        protected override void Redirect(HttpContext context, PaymentResponse response)
        {
           

            if (response.PaymentServiceId != null)
            {
                switch (response.PaymentServiceId.Value)
                {
                    case 1: //Payex
                        RedirectBasic(context, response);
                        break;
                    case 4: //Dibs
                        RedirectForm(context, response);
                        break;
                    case 9: //Paynova
                        RedirectBasic(context, response);
                        break;                       
                    case 11: //Resurs Bank
                        RedirectResursBank(context, response);
                          break;
                    case 15: //Verifone
                        RedirectForm(context, response);
                        break;
                    case 16: //Adyen
                        RedirectForm(context, response);
                        break;
                }              
            }
            else
            {
                throw new ApplicationException("Fel i betalningen.");
            }
        }

        private void RedirectBasic(HttpContext context, PaymentResponse response)
        {
            context.Response.Redirect(response.RedirectUrl, false);
            context.ApplicationInstance.CompleteRequest();
        }

        private void RedirectForm(HttpContext context, PaymentResponse response)
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

        private void RedirectResursBank(HttpContext context, PaymentResponse response)
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
