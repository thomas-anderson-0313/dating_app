using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Java.Lang;
using Newtonsoft.Json;
using QuickDate.Activities.Tabbes;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Utils;
using QuickDate.SQLite;
using QuickDateClient;
using QuickDateClient.Classes.Chat;
using QuickDateClient.Requests;
using Exception = System.Exception;

namespace QuickDate.Activities.Chat.Service
{
    [Service(Exported = false)]
    public class ChatApiService : JobIntentService
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                //Toast.MakeText(Application.Context, "OnCreate", ToastLength.Short)?.Show();
                new Handler(Looper.MainLooper).PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnHandleWork(Intent p0)
        {
            try
            {
                //Toast.MakeText(Application.Context, "OnHandleWork", ToastLength.Short)?.Show();
                new Handler(Looper.MainLooper).PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                base.OnStartCommand(intent, flags, startId);

                new Handler(Looper.MainLooper).PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);

                //Toast.MakeText(Application.Context, "OnStartCommand", ToastLength.Short)?.Show();

                return StartCommandResult.Sticky;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return StartCommandResult.NotSticky;
            }
        }
    }

    public class PostUpdaterHelper : Java.Lang.Object, IRunnable
    {
        private static Handler MainHandler;

        public PostUpdaterHelper(Handler mainHandler)
        {
            MainHandler = mainHandler;
        }

        public void Run()
        {
            try
            {
                if (string.IsNullOrEmpty(Methods.AppLifecycleObserver.AppState))
                    Methods.AppLifecycleObserver.AppState = "Background";

                //Toast.MakeText(Application.Context, "AppState " + Methods.AppLifecycleObserver.AppState, ToastLength.Short)?.Show();

                if (Methods.AppLifecycleObserver.AppState == "Background" && string.IsNullOrEmpty(Current.AccessToken))
                {
                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    var login = dbDatabase.Get_data_Login_Credentials();
                    Console.WriteLine(login);
                }

                if (string.IsNullOrEmpty(Current.AccessToken))
                    return;

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadChatAsync });

                MainHandler ??= new Handler(Looper.MainLooper);
                MainHandler?.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                //Toast.MakeText(Application.Context, "ResultSender failed",ToastLength.Short)?.Show();
                MainHandler ??= new Handler(Looper.MainLooper);
                MainHandler?.PostDelayed(new PostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);
                Methods.DisplayReportResultTrack(e);
            }   
        }
         
        private async Task LoadChatAsync()
        {
            try
            {
                //Toast.MakeText(Application.Context, "StartApiService", ToastLength.Short)?.Show();
                //if (LastChatFragment.ApiRun)
                //    return;

                //LastChatFragment.ApiRun = true;

                var (apiStatus, respond) = await RequestsAsync.Chat.GetConversationListAsync("35", "0");
                if (apiStatus != 200 || respond is not GetConversationListObject result || result.Data == null)
                {
                    //LastChatFragment.ApiRun = false;
                    //Methods.DisplayReportResult(new Activity(), respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (Methods.AppLifecycleObserver.AppState == "Foreground")
                        {
                            HomeActivity.GetInstance()?.OnReceiveResult(JsonConvert.SerializeObject(result));
                        }
                        else
                        {
                            ListUtils.ChatList = new ObservableCollection<GetConversationListObject.DataConversation>(result.Data);

                            //Insert All data users to database
                            SqLiteDatabase dbDatabase = new SqLiteDatabase(); 
                            dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);

                            //LastChatFragment.LoadCall(result);
                            //LastChatFragment.ApiRun = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                //LastChatFragment.ApiRun = false;
            }
        }

    }
} 