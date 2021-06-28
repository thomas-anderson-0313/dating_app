using System;
using System.Collections.Generic;
using Android.App;
using Android.Content; 
using Android.Widget;
using Com.OneSignal.Abstractions;
using Newtonsoft.Json;
using QuickDate.Activities.Tabbes;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDateClient.Classes.Global;
using OSNotification = Com.OneSignal.Abstractions.OSNotification;
using OSNotificationPayload = Com.OneSignal.Abstractions.OSNotificationPayload;

namespace QuickDate.Library.OneSignal
{
    public static class OneSignalNotification
    {
        //Force your app to Register Notification directly without loading it from server (For Best Result)

        public static string Type;
        public static UserInfoObject UserData;

        public static void RegisterNotificationDevice()
        {
            try
            {
                if (UserDetails.NotificationPopup)
                {
                    if (!string.IsNullOrEmpty(AppSettings.OneSignalAppId) || !string.IsNullOrWhiteSpace(AppSettings.OneSignalAppId))
                    {
                        Com.OneSignal.OneSignal.Current.StartInit(AppSettings.OneSignalAppId)
                            .InFocusDisplaying(OSInFocusDisplayOption.Notification)
                            .HandleNotificationReceived(HandleNotificationReceived)
                            .HandleNotificationOpened(HandleNotificationOpened)
                            .EndInit();
                        Com.OneSignal.OneSignal.Current.IdsAvailable(IdsAvailable);
                        Com.OneSignal.OneSignal.Current.RegisterForPushNotifications();
                        Com.OneSignal.OneSignal.Current.SetSubscription(true);
                        AppSettings.ShowNotification = true;
                    }
                }
                else
                {
                    UnRegisterNotificationDevice();
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public static void UnRegisterNotificationDevice()
        {
            try
            {
                Com.OneSignal.OneSignal.Current.SetSubscription(false);
                AppSettings.ShowNotification = false;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private static void IdsAvailable(string userId, string pushToken)
        {
            try
            {
                UserDetails.DeviceId = userId;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private static void HandleNotificationReceived(OSNotification notification)
        {
            try
            {

                OSNotificationPayload payload = notification.payload;
                Dictionary<string, object> additionalData = payload.additionalData;

                string message = payload.body;

            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, ex.ToString(), ToastLength.Long)?.Show(); //Allen
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private static void HandleNotificationOpened(OSNotificationOpenedResult result)
        {
            try
            {
                OSNotificationPayload payload = result.notification.payload;
                Dictionary<string, object> additionalData = payload.additionalData;
                string message = payload.body;
                string actionId = result.action.actionID;

                if (additionalData != null)
                {
                    foreach (var item in additionalData)
                    {
                        switch (item.Key)
                        {
                            case "type":
                                Type = item.Value.ToString();
                                break;
                            case "userdata":
                                UserData = JsonConvert.DeserializeObject<UserInfoObject>(item.Value.ToString());
                                break;
                        }
                    }

                    //to : do
                    //go to activity or fragment depending on data

                    Intent intent = new Intent(Application.Context, typeof(HomeActivity));
                    intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    intent.AddFlags(ActivityFlags.SingleTop);
                    intent.SetAction(Intent.ActionView);
                    intent.PutExtra("TypeNotification", Type);
                    Application.Context.StartActivity(intent);
                      
                    if (additionalData.ContainsKey("discount"))
                    {
                        // Take user to your store..

                    }
                }

                if (actionId != null)
                {
                    // actionSelected equals the id on the button the user pressed.
                    // actionSelected will equal "__DEFAULT__" when the notification itself was tapped when buttons were present.  
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        } 
    }
}