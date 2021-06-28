﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Auth.Api;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Java.IO;
using Newtonsoft.Json;
using QuickDate.Activities.Call;
using QuickDate.Activities.Chat.Service;
using QuickDate.Activities.Default;
using QuickDate.Activities.SettingsUser;
using QuickDate.Activities.Tabbes;
using QuickDate.Helpers.CacheLoaders;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDate.Library.OneSignal;
using QuickDate.SQLite;
using QuickDateClient;
using QuickDateClient.Classes.Common;
using QuickDateClient.Classes.Global;
using QuickDateClient.Classes.Users;
using QuickDateClient.Requests;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Console = System.Console;

namespace QuickDate.Helpers.Controller
{
    internal static class ApiRequest
    {
        public static async Task GetSettings_Api(Activity activity)
        {
            if (Methods.CheckConnectivity())
            {
                await GetLanguages_Api(activity);

                var lang = ListUtils.LanguagesSiteList.FirstOrDefault(languages => languages.LanguagesName.ToLower() == UserDetails.LangName)?.LanguagesName ?? "";//english

                var (apiStatus, respond) = await Current.GetOptionsAsync(lang);
                if (apiStatus == 200)
                {
                    if (respond is GetOptionsObject result)
                    {
                        if (result.Data != null)
                        {
                            AppSettings.OneSignalAppId = result.Data.PushId;
                            OneSignalNotification.RegisterNotificationDevice();

                            ListUtils.SettingsSiteList = result.Data;

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrUpdateSettings(result.Data);
                            
                        }
                    }
                }
                //else Methods.DisplayReportResult(activity, respond);
            }
        }

        private static async Task GetLanguages_Api(Activity activity)
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Auth.GetLanguagesAsync();
                if (apiStatus == 200)
                {
                    if (respond is GetLanguagesObject result)
                    {
                        if (result.Data?.Count > 0)
                        {
                            var listLanguages = result.Data.Select(cat => new Classes.Languages
                            {
                                LanguagesId = cat.Keys.FirstOrDefault(),
                                LanguagesName = cat.Values.FirstOrDefault(),
                            }).ToList();

                            ListUtils.LanguagesSiteList.Clear();
                            ListUtils.LanguagesSiteList = new ObservableCollection<Classes.Languages>(listLanguages);
                        }
                    }
                }
                //else Methods.DisplayReportResult(activity, respond);
            }
        }

        public static async Task<string> GetTimeZoneAsync()
        {
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync("http://ip-api.com/json/");
                string json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<TimeZoneObject>(json);
                return data?.Timezone ?? "UTC"; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "UTC";
            }
        }

        public static async Task<ProfileObject> GetInfoData(Activity context, string userId)
        {
            if (Methods.CheckConnectivity())
            {
                int apiStatus;
                dynamic respond;

                if (userId == UserDetails.UserId.ToString())
                    (apiStatus, respond) = await RequestsAsync.Users.ProfileAsync(userId);
                else
                    (apiStatus, respond) = await RequestsAsync.Users.ProfileAsync(userId, "data,media"); 
                if (apiStatus == 200)
                {
                    if (respond is ProfileObject result)
                    {
                        if (userId == UserDetails.UserId.ToString())
                        { 
                            UserDetails.Avatar = result.Data.Avater.PurpleUri;
                            UserDetails.Username = result.Data.Username;
                            UserDetails.FullName = result.Data.Fullname;
                            UserDetails.IsPro = result.Data.IsPro;
                            UserDetails.Email = result.Data.Email;
                            UserDetails.Url = Client.WebsiteUrl + "@" + result.Data?.Username;
                           
                            ListUtils.MyUserInfo.Clear();
                            ListUtils.MyUserInfo.Add(result.Data);

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrUpdate_DataMyInfo(result.Data);
                            

                            try { GlideImageLoader.LoadImage(context, UserDetails.Avatar, HomeActivity.GetInstance()?.FragmentBottomNavigator?.ProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable); }catch (Exception e) { Methods.DisplayReportResultTrack(e); }

                            await Task.Factory.StartNew(async () =>
                            {
                                try
                                {
                                    var respondList = result.Data.Mediafiles.Count;
                                    if (respondList > 0)
                                    {
                                        foreach (var item in result.Data.Mediafiles)
                                        {
                                            Glide.With(context).Load(item.Avater).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                             
                                            if (item.IsVideo == "1")
                                            {
                                                var fileName = item.VideoFile.Split('/').Last();
                                                item.UrlFile = await QuickDateTools.GetFile(UserDetails.UserId.ToString(), Methods.Path.FolderDiskVideo, fileName, item.VideoFile);
                                            }
                                            else 
                                            {
                                                var fileName = item.Full.Split('/').Last();
                                                item.UrlFile = await QuickDateTools.GetFile(UserDetails.UserId.ToString(), Methods.Path.FolderDiskImage, fileName, item.Full);
                                            }
                                        }  
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            }).ConfigureAwait(false); 
                       
                            return result;
                        }
                        else
                        {
                            return result;
                        }
                    }
                }
                //else Methods.DisplayReportResult(context, respond);
            }

            return null!;
        }

        private static bool CallActionPopupOpened;
        public static async Task<(long, long)> GetCountNotifications(Activity activity)
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Common.GetNotificationsAsync("25", "", UserDetails.DeviceId);
                if (apiStatus == 200)
                {
                    if (respond is not GetNotificationsObject result) return (0, 0);
                     
                    var dataSettings = ListUtils.SettingsSiteList;

                    if (dataSettings?.AudioChat == "1" || dataSettings?.VideoChat == "1")
                    {
                        if (dataSettings.AudioChat == "1")
                        {
                            if (result.AudioCall != null && result.AudioCall.Value.DatumClass != null && !CallActionPopupOpened)
                            {
                                var data = result.AudioCall.Value.DatumClass;

                                string name = !string.IsNullOrEmpty(data.Fullname) ? data.Fullname : data.Username;

                                Intent intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                                intent.PutExtra("type", "Twilio_audio_call");

                                intent.PutExtra("UserID", data.ToId);
                                intent.PutExtra("avatar", data.Avater);
                                intent.PutExtra("name", name);
                                intent.PutExtra("access_token", data.AccessToken);
                                intent.PutExtra("access_token_2", data.AccessToken2);
                                intent.PutExtra("from_id", data.FromId);
                                intent.PutExtra("active", data.Active);
                                intent.PutExtra("time", data.Time);
                                intent.PutExtra("CallID", data.Id);
                                intent.PutExtra("status", data.Declined);
                                intent.PutExtra("room_name", data.RoomName);
                                intent.PutExtra("declined", data.Declined);

                                string avatarSplit = data.Avater.Split('/').Last();
                                var getImg = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage, avatarSplit);
                                if (getImg == "File Dont Exists")
                                    Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDiskImage, data.Avater);

                                if (!VideoAudioComingCallActivity.IsActive)
                                {
                                    intent.AddFlags(ActivityFlags.NewTask);
                                    activity.StartActivity(intent);
                                }
                            }
                            else
                            {
                                if (CallActionPopupOpened)
                                {
                                    CallActionPopupOpened = false;
                                    if (VideoAudioComingCallActivity.IsActive)
                                        VideoAudioComingCallActivity.CallActivity?.Finish();
                                }
                            }
                        }

                        if (dataSettings.VideoChat == "1")
                        {
                            if (result.VideoCall != null && result.VideoCall.Value.DatumClass != null && !CallActionPopupOpened)
                            {
                                var data = result.VideoCall.Value.DatumClass;
                                string name = !string.IsNullOrEmpty(data.Fullname) ? data.Fullname : data.Username;

                                Intent intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                                intent.PutExtra("type", "Twilio_video_call");

                                intent.PutExtra("UserID", data.ToId);
                                intent.PutExtra("avatar", data.Avater);
                                intent.PutExtra("name", name);
                                intent.PutExtra("access_token", data.AccessToken);
                                intent.PutExtra("access_token_2", data.AccessToken2);
                                intent.PutExtra("from_id", data.FromId);
                                intent.PutExtra("active", data.Active);
                                intent.PutExtra("time", data.Time);
                                intent.PutExtra("CallID", data.Id);
                                intent.PutExtra("status", data.Declined);
                                intent.PutExtra("room_name", data.RoomName);
                                intent.PutExtra("declined", data.Declined);

                                string avatarSplit = data.Avater.Split('/').Last();
                                var getImg = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDiskImage, avatarSplit);
                                if (getImg == "File Dont Exists")
                                    Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDiskImage, data.Avater);

                                if (!VideoAudioComingCallActivity.IsActive)
                                {
                                    intent.AddFlags(ActivityFlags.NewTask);
                                    activity.StartActivity(intent);
                                }
                            }
                            else
                            {
                                if (CallActionPopupOpened)
                                {
                                    CallActionPopupOpened = false;
                                    if (VideoAudioComingCallActivity.IsActive)
                                        VideoAudioComingCallActivity.CallActivity?.Finish();
                                }
                            }
                        }
                    }

                    return (result.NewNotificationCount, result.NewMessagesCount);
                }
                else Methods.DisplayReportResult(activity, respond);
            }
            return (0, 0);
        }

        public static async Task UpdateAvatarApi(Activity activity, string path)
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Users.UpdateAvatarAsync(path);
                if (apiStatus == 200)
                {
                    if (respond is UpdateAvatarObject result)
                    {
                        Console.WriteLine(result.Message);
                        var local = ListUtils.MyUserInfo?.FirstOrDefault();
                        if (local != null)
                        {
                            local.Avater = path;
                            UserDetails.Avatar = path;

                            SqLiteDatabase database = new SqLiteDatabase();
                            database.InsertOrUpdate_DataMyInfo(local);
                            
                        }
                    }
                }
                else Methods.DisplayReportResult(activity, respond);
            }
            else
            {
                Toast.MakeText(activity, activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }
         
        public static async Task GetGifts(Activity activity)
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Common.GetGiftsAsync().ConfigureAwait(false);
                if (apiStatus == 200)
                {
                    if (respond is GetGiftsObject result)
                    {
                        if (result.Data.Count > 0)
                        {
                            ListUtils.GiftsList.Clear();
                            ListUtils.GiftsList = new ObservableCollection<DataFile>(result.Data);
                             
                            SqLiteDatabase sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.InsertAllGifts(ListUtils.GiftsList);
                            

                            await Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    foreach (var item in result.Data)
                                    {
                                        var url = item.File.Contains("media3.giphy.com/");
                                        if (url)
                                        {
                                            item.File = item.File.Replace(Client.WebsiteUrl, "");
                                        }

                                        Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDiskGif, item.File);

                                        Glide.With(activity).Load(item.File).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            }).ConfigureAwait(false); 
                        }
                    }
                }
                else Methods.DisplayReportResult(activity, respond);
            }
            else
            {
                Toast.MakeText(activity, activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        public static async Task GetStickers(Activity activity)
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Common.GetStickersAsync();
                if (apiStatus == 200)
                {
                    if (respond is GetStickersObject result)
                    {
                        if (result.Data.Count > 0)
                        {
                            ListUtils.StickersList.Clear();
                            ListUtils.StickersList = new ObservableCollection<DataFile>(result.Data);

                            SqLiteDatabase sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.InsertAllStickers(ListUtils.StickersList);
                            

                            await Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    foreach (var item in result.Data)
                                    {
                                        var url = item.File.Contains("media3.giphy.com/");
                                        if (url)
                                        {
                                            item.File = item.File.Replace(Client.WebsiteUrl, "");
                                        }

                                        Methods.MultiMedia.DownloadMediaTo_DiskAsync(Methods.Path.FolderDiskSticker, item.File);

                                        Glide.With(activity).Load(item.File).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            }).ConfigureAwait(false);
                        }
                    }
                }
                else Methods.DisplayReportResult(activity, respond);
            }
            else
            {
                Toast.MakeText(activity, activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        //Get General Data Using Api >> Friend Requests
        public static async Task<string> LoadFriendRequestsData(HomeActivity activity)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Friends.GetFriendsRequestsAsync("30");
                    if (apiStatus != 200 || respond is not ListUsersObject result || result.Data == null)
                    {
                        Methods.DisplayReportResult(activity, respond);
                    }
                    else
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            ListUtils.FriendRequestsList = new ObservableCollection<UserInfoObject>(result.Data); 
                            //activity?.ShowOrHideBadgeView(result.Data.Count, true); 
                            return result.Data.Count.ToString();
                        }
                    } 
                }
                //activity?.ShowOrHideBadgeView();
                return "0";
            }
            catch (Exception e)
            {
               // activity?.ShowOrHideBadgeView();
                Methods.DisplayReportResultTrack(e);
                return "0";
            }
        }
         

        //=====================================
        public static bool RunLogout;

        public static async void Delete(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    RunLogout = true;

                    await RemoveData("Delete");

                    context?.RunOnUiThread(() =>
                    {
                        Methods.Path.DeleteAll_MyFolderDisk();

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();

                        Java.Lang.Runtime.GetRuntime()?.RunFinalization();
                        Java.Lang.Runtime.GetRuntime()?.Gc();
                        TrimCache(context);

                        dbDatabase.ClearAll();
                        dbDatabase.DropAll();

                        ListUtils.ClearAllList();

                        UserDetails.ClearAllValueUserDetails();

                        dbDatabase.CheckTablesStatus();
                        

                        var intentService = new Intent(context, typeof(ChatApiService));
                        context.StopService(intentService);

                       MainSettings.SharedData?.Edit()?.Clear()?.Commit();

                        Intent intent = new Intent(context, typeof(FirstActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        context.StartActivity(intent);
                        context.FinishAffinity();
                        context.Finish();
                    }); 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async void Logout(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    RunLogout = true;

                    await RemoveData("Logout");

                    var intentService = new Intent(context, typeof(ChatApiService));
                    context.StopService(intentService);

                    HomeActivity.GetInstance().CardFragment.MainHandler.RemoveCallbacks(HomeActivity.GetInstance().CardFragment.Runnable);
                    HomeActivity.GetInstance().CardFragment.MainHandler = null;

                    context?.RunOnUiThread(() =>
                    {
                        Methods.Path.DeleteAll_MyFolderDisk();

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();

                        Java.Lang.Runtime.GetRuntime()?.RunFinalization();
                        Java.Lang.Runtime.GetRuntime()?.Gc();
                        TrimCache(context);

                        dbDatabase.ClearAll();
                        dbDatabase.DropAll();

                        ListUtils.ClearAllList();

                        UserDetails.ClearAllValueUserDetails();

                        dbDatabase.CheckTablesStatus();
                        
                         
                        MainSettings.SharedData?.Edit()?.Clear()?.Commit();

                        Intent intent = new Intent(context, typeof(FirstActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        context.StartActivity(intent);
                        context.FinishAffinity();
                        context.Finish();
                    }); 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static void TrimCache(Activity context)
        {
            try
            {
                File dir = context.CacheDir;
                if (dir != null && dir.IsDirectory)
                {
                    DeleteDir(dir);
                }

                context.DeleteDatabase(AppSettings.DatabaseName + "_.db");
                context.DeleteDatabase(SqLiteDatabase.PathCombine);

                if (context?.IsDestroyed != false)
                    return;

                Glide.Get(context)?.ClearMemory();
                new Thread(() =>
                {
                    try
                    {
                        Glide.Get(context)?.ClearDiskCache();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }).Start(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static bool DeleteDir(File dir)
        {
            try
            {
                if (dir == null || !dir.IsDirectory) return dir != null && dir.Delete();
                string[] children = dir.List();
                if (children.Select(child => DeleteDir(new File(dir, child))).Any(success => !success))
                {
                    return false;
                }

                // The directory is now empty so delete it
                return dir.Delete();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        private static async Task RemoveData(string type)
        {
            try
            {
                switch (type)
                {
                    case "Logout":
                    {
                        if (Methods.CheckConnectivity())
                        {
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { RequestsAsync.Auth.LogoutAsync }); 
                        }

                        break;
                    }
                    case "Delete":
                    {
                        Methods.Path.DeleteAll_FolderUser();    
                        if (Methods.CheckConnectivity())
                        {
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Auth.DeleteAccountAsync(UserDetails.Password) }); 
                        }

                        break;
                    }
                }

                try
                {
                    if (AppSettings.ShowGoogleLogin && SocialLoginBaseActivity.MGoogleSignInClient != null)
                        if (Auth.GoogleSignInApi != null)
                        {
                            SocialLoginBaseActivity.MGoogleSignInClient.SignOut();
                            SocialLoginBaseActivity.MGoogleSignInClient = null!;
                        }

                    if (AppSettings.ShowFacebookLogin)
                    {
                        var accessToken = AccessToken.CurrentAccessToken;
                        var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                        if (isLoggedIn && Profile.CurrentProfile != null)
                        {
                            LoginManager.Instance.LogOut();
                        }
                    }
                     
                    OneSignalNotification.UnRegisterNotificationDevice(); 
                    ListUtils.ClearAllList(); 
                    UserDetails.ClearAllValueUserDetails(); 

                    GC.Collect();
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }

                await Task.Delay(0);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}