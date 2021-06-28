using System;
using Android.App;
using Android.Content;
using Java.Math;
using QuickDate.Helpers.Utils;
using QuickDateClient;
using Xamarin.PayPal.Android;

namespace QuickDate.PaymentGoogle
{
    public class InitPayPalPayment
    {
        private readonly Activity ActivityContext;
        private static PayPalConfiguration PayPalConfig;
        private PayPalPayment PayPalPayment;
        private Intent IntentService;
        public string Price, PayType, Credits, Id;
        public readonly int PayPalDataRequestCode = 7171;

        public InitPayPalPayment(Activity activity)
        {
            ActivityContext = activity;
        }

        //Paypal
        public void BtnPaypalOnClick(string price, string payType, string credits, string id)
        {
            try
            {
                InitPayPal(price, payType, credits, id);

                Intent intent = new Intent(ActivityContext, typeof(PaymentActivity));
                intent.PutExtra(PayPalService.ExtraPaypalConfiguration, PayPalConfig);
                intent.PutExtra(PaymentActivity.ExtraPayment, PayPalPayment);
                ActivityContext.StartActivityForResult(intent, PayPalDataRequestCode);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void InitPayPal(string price, string payType, string credits, string id)
        {
            try
            {
                Price = price; PayType = payType; Credits = credits; Id = id;

                //PayerID
                string currency = "USD";
                string paypalClintId = "";
                var option = ListUtils.SettingsSiteList;
                if (option != null)
                {
                    currency = option?.Currency ?? "USD";
                    paypalClintId = option?.PaypalId; 
                }

                PayPalConfig = new PayPalConfiguration()
                    .ClientId(paypalClintId)
                    .LanguageOrLocale(AppSettings.Lang)
                    .MerchantName(AppSettings.ApplicationName)
                    .MerchantPrivacyPolicyUri(Android.Net.Uri.Parse(Client.WebsiteUrl + "/terms/privacy-policy"));

                if (option != null)
                {
                    switch (option.PaypalMode)
                    {
                        case "sandbox":
                            PayPalConfig.Environment(PayPalConfiguration.EnvironmentSandbox);
                            break;
                        case "live":
                            PayPalConfig.Environment(PayPalConfiguration.EnvironmentProduction);
                            break;
                        default:
                            PayPalConfig.Environment(PayPalConfiguration.EnvironmentProduction);
                            break;
                    }
                }

                PayPalPayment = new PayPalPayment(new BigDecimal(price), currency, "Pay the card", PayPalPayment.PaymentIntentSale);

                IntentService = new Intent(ActivityContext, typeof(PayPalService)); 
                IntentService.PutExtra(PayPalService.ExtraPaypalConfiguration, PayPalConfig);
                ActivityContext.StartService(IntentService);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StopPayPalService()
        {
            try
            {
                ActivityContext.StopService(new Intent(ActivityContext, typeof(PayPalService)));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}