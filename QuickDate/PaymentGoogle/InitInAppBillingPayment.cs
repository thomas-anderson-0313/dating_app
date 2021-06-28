using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Widget;
using Plugin.CurrentActivity;
using QuickDate.Helpers.Utils;
using QuickDateClient;
using Xamarin.InAppBilling;

namespace QuickDate.PaymentGoogle
{
    public class InitInAppBillingPayment
    {
        private readonly Activity ActivityContext;
        public SaneInAppBillingHandler Handler;
        private IReadOnlyList<Product> Products;
        public string Price, PayType, Credits, Id;

        public InitInAppBillingPayment(Activity activity)
        {
            ActivityContext = activity;
        }

        #region In-App Billing Google

        public async void SetConnInAppBilling()
        {
            try
            {
                CrossCurrentActivity.Current.Activity = ActivityContext;
                Handler = new SaneInAppBillingHandler(ActivityContext, InAppBillingGoogle.ProductId);
                // Call this method when creating your activity
                await Handler.Connect();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisconnectInAppBilling()
        {
            try
            {
                Handler?.Disconnect();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void InitInAppBilling(string price, string payType, string credits, string id)
        {
            Price = price; PayType = payType; Credits = credits; Id = id;

            if (Methods.CheckConnectivity())
            {
                if (!Handler.ServiceConnection.Connected)
                {
                    // Call this method when creating your activity 
                    await Handler.Connect();
                }

                try
                {
                    Products = await Handler.QueryInventory(InAppBillingGoogle.ListProductSku, ItemType.Product);
                    if (Products?.Count > 0)
                    {
                        // Ask the open connection's billing handler to get any purchases 
                        var purchases = Handler.ServiceConnection.BillingHandler.GetPurchases(ItemType.Product);

                        var hasPaid = purchases != null && purchases.Any();
                        if (hasPaid)
                        {
                            var chk = purchases.FirstOrDefault(a => a.ProductId == Products[0].ProductId);
                            if (chk != null)
                            {
                                bool result = Handler.ServiceConnection.BillingHandler.ConsumePurchase(chk);
                                if (result)
                                {
                                    Console.WriteLine(chk);
                                }
                            }
                        }
                        var option = ListUtils.SettingsSiteList;

                        var bagOfCredits = Products.FirstOrDefault(a => a.ProductId == "bagofcredits");
                        var boxOfCredits = Products.FirstOrDefault(a => a.ProductId == "boxofcredits");
                        var chestOfCredits = Products.FirstOrDefault(a => a.ProductId == "chestofcredits");
                        var memberShipWeekly = Products.FirstOrDefault(a => a.ProductId == "membershipweekly");
                        var membershipMonthly = Products.FirstOrDefault(a => a.ProductId == "membershipmonthly");
                        var membershipYearly = Products.FirstOrDefault(a => a.ProductId == "membershipyearly");
                        var membershipLifeTime = Products.FirstOrDefault(a => a.ProductId == "membershiplifetime");
                         
                        switch (PayType)
                        {
                            case "credits" when Credits == option?.BagOfCreditsAmount:
                                await Handler.BuyProduct(bagOfCredits);
                                break;
                            case "credits" when Credits == option?.BoxOfCreditsAmount:
                                await Handler.BuyProduct(boxOfCredits);
                                break;
                            case "credits" when Credits == option?.ChestOfCreditsAmount:
                                await Handler.BuyProduct(chestOfCredits); 
                                break;
                            //Weekly
                            case "membership" when Id == "1":
                                await Handler.BuyProduct(memberShipWeekly);
                                break;
                            //Monthly
                            case "membership" when Id == "2":
                                await Handler.BuyProduct(membershipMonthly);
                                break;
                            //Yearly
                            case "membership" when Id == "3":
                                await Handler.BuyProduct(membershipYearly);
                                break;
                            case "membership" when Id == "4": 
                                await Handler.BuyProduct(membershipLifeTime);
                                break; 
                        }

                        Handler.ServiceConnection.BillingHandler.OnProductPurchased += delegate (int response, Purchase purchase, string data, string signature)
                        {
                            try
                            {
                                if (response == BillingResult.OK)
                                {
                                    //Sent APi
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        };

                        // Attach to the various error handlers to report issues
                        Handler.ServiceConnection.BillingHandler.OnGetProductsError += (responseCode, ownedItems) =>
                        {
                            Console.WriteLine("Error getting products");
                            Toast.MakeText(ActivityContext, "Error getting products ", ToastLength.Long)?.Show();
                        };

                        Handler.ServiceConnection.BillingHandler.OnInvalidOwnedItemsBundleReturned += (ownedItems) =>
                        {
                            Console.WriteLine("Invalid owned items bundle returned");
                            Toast.MakeText(ActivityContext, "Invalid owned items bundle returned ", ToastLength.Long)?.Show();
                        };

                        Handler.ServiceConnection.BillingHandler.OnProductPurchasedError += (responseCode, sku) =>
                        {
                            Console.WriteLine("Error purchasing item {0}", sku);
                            Toast.MakeText(ActivityContext, "Error purchasing item " + sku, ToastLength.Long)?.Show();
                        };

                        Handler.ServiceConnection.BillingHandler.OnPurchaseConsumedError += (responseCode, token) =>
                        {
                            Console.WriteLine("Error consuming previous purchase");
                            Toast.MakeText(ActivityContext, "Error consuming previous purchase ", ToastLength.Long)?.Show();
                        };

                        Handler.ServiceConnection.BillingHandler.InAppBillingProcesingError += (message) =>
                        {
                            Console.WriteLine("In app billing processing error {0}", message);
                            Toast.MakeText(ActivityContext, "In app billing processing error " + message, ToastLength.Long)?.Show();
                        };

                        Handler.ServiceConnection.BillingHandler.OnPurchaseConsumed += delegate (string token)
                        {
                            Toast.MakeText(ActivityContext, "In app billing processing error " + token, ToastLength.Long)?.Show();
                            Console.WriteLine("In app billing processing error {0}", token);
                        };

                        Handler.ServiceConnection.BillingHandler.BuyProductError += delegate (int code, string sku)
                        {
                            Toast.MakeText(ActivityContext, "There is something wrong please try again later", ToastLength.Long)?.Show();
                        };

                        Handler.ServiceConnection.BillingHandler.QueryInventoryError += delegate (int code, Bundle details) { };
                    }
                }
                catch (Exception ex)
                {
                    //Something else has gone wrong, log it
                    Methods.DisplayReportResultTrack(ex);
                }
                finally
                {
                    Handler.Disconnect();
                }
            }
            else
            {
                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
            }
        }

        #endregion

    }
}