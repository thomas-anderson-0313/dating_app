using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using QuickDate.Library.Anjo.IntegrationRecyclerView;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using QuickDate.Activities.Base;
using QuickDate.Activities.Chat.Adapters;
using QuickDate.Activities.Tabbes;
using QuickDate.Helpers.Ads;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDate.SQLite;
using QuickDateClient.Classes.Chat;
using QuickDateClient.Requests;
using ActionMode = AndroidX.AppCompat.View.ActionMode;
using Object = Java.Lang.Object;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace QuickDate.Activities.Chat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class LastChatActivity : BaseActivity, IOnClickListenerSelected
    {
        #region Variables Basic

        public static LastChatAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private static ActionMode ActionMode;
        private static Toolbar ToolBar;
        private AdView MAdView;
        private ActionModeCallback ModeCallback;
        private string UserId = "";
        private static LastChatActivity Instance;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                 
                GetLastChatLocal();

                AdsGoogle.Ad_Interstitial(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                MAdView?.Resume();
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                MAdView?.Pause();
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                ListUtils.ChatList = MAdapter.UserList;

                MAdapter?.UserList.Clear();
                MAdapter?.NotifyDataSetChanged();

                MAdView?.Destroy();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                var mainLayout = FindViewById<CoordinatorLayout>(Resource.Id.mainLayout);
                mainLayout.SetPadding(0, 0, 0, 0);

                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.SetPadding(5, 0, 0, 0);

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new LastChatAdapter(this);
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                //MRecycler.HasFixedSize = true;
                //MRecycler.SetItemViewCacheSize(10);
                //MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<GetConversationListObject.DataConversation>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
                MAdapter.SetOnClickListener(this);

                ModeCallback = new ActionModeCallback(this);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = GetText(Resource.String.Lbl_Chats);
                    ToolBar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? AppSettings.TitleTextColorDark : AppSettings.TitleTextColor);
                    SetSupportActionBar(ToolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    ToolBar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
  
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static LastChatActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        #endregion

        #region Scroll

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                //Event Scroll #LastChat
                var item = MAdapter.UserList.LastOrDefault();
                if (item != null && MAdapter.UserList.Count > 10)
                {
                    StartApiService(item.Id.ToString());
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Toolbar & Selected

        public class ActionModeCallback : Object, ActionMode.ICallback
        {
            private readonly LastChatActivity Activity;
            public ActionModeCallback(LastChatActivity activity)
            {
                Activity = activity;
            }

            public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
            {
                int id = item.ItemId;
                switch (id)
                {
                    case Resource.Id.action_delete:
                        DeleteItems();
                        mode.Finish();
                        return true;
                    case Android.Resource.Id.Home:
                        HomeActivity.GetInstance()?.SetService();

                        MAdapter.ClearSelections();

                        ToolBar.Visibility = ViewStates.Visible;
                        ActionMode.Finish();

                        return true;
                    default:
                        return false;
                }
            }

            public bool OnCreateActionMode(ActionMode mode, IMenu menu)
            {
                SetSystemBarColor(Activity, AppSettings.MainColor);
                mode.MenuInflater.Inflate(Resource.Menu.menu_delete, menu);
                return true;
            }

            public void SetSystemBarColor(Activity act, string color)
            {
                try
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Window window = act.Window;
                        window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        window.SetStatusBarColor(Color.ParseColor(color));
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnDestroyActionMode(ActionMode mode)
            {
                try
                {
                    MAdapter.ClearSelections();
                    ActionMode.Finish();
                    ActionMode = null;

                    SetSystemBarColor(Activity, AppSettings.MainColor);

                    HomeActivity.GetInstance()?.SetService();

                    ToolBar.Visibility = ViewStates.Visible;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
            {
                return false;
            }

            //Delete Chat 
            private void DeleteItems()
            {
                try
                {
                    HomeActivity.GetInstance()?.SetService();

                    if (ToolBar.Visibility != ViewStates.Visible)
                        ToolBar.Visibility = ViewStates.Visible;

                    if (Methods.CheckConnectivity())
                    {
                        List<int> selectedItemPositions = MAdapter.GetSelectedItems();
                        for (int i = selectedItemPositions.Count - 1; i >= 0; i--)
                        {
                            var datItem = MAdapter.GetItem(selectedItemPositions[i]);
                            if (datItem != null)
                            {
                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.DeleteUserLastChat(datItem.User.Id.ToString());
                                dbDatabase.DeleteAllMessagesUser(UserDetails.UserId.ToString(), datItem.User.Id.ToString());
                                

                                var index = MAdapter.UserList.IndexOf(MAdapter.UserList.FirstOrDefault(a => a.User.Id == datItem.User.Id));
                                if (index != -1)
                                {
                                    MAdapter.UserList.Remove(datItem);
                                    MAdapter.NotifyItemRemoved(index);
                                }

                                MAdapter.RemoveData();

                                //Send Api Delete 
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Chat.DeleteMessagesAsync(datItem.User.Id.ToString()) });
                            }
                        }
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        //Event 
        public void ItemClick(View view, GetConversationListObject.DataConversation obj, int pos)
        {
            try
            {
                if (MAdapter.GetSelectedItemCount() > 0) // Add Select  New Item 
                {
                    EnableActionMode(pos);
                }
                else
                {
                    HomeActivity.GetInstance()?.SetService();

                    if (ToolBar.Visibility != ViewStates.Visible)
                        ToolBar.Visibility = ViewStates.Visible;

                    // read the item which removes bold from the row >> event click open ChatBox by user id
                    var item = MAdapter.GetItem(pos);
                    if (item != null)
                    {
                        UserId = item.User.Id.ToString();
                        item.NewMessages = 0;
                        item.Seen = 1;
                        Intent intent = new Intent(this, typeof(MessagesBoxActivity));
                        intent.PutExtra("UserId", item.User.Id.ToString());
                        intent.PutExtra("TypeChat", "LastChat");
                        intent.PutExtra("UserItem", JsonConvert.SerializeObject(item.User));

                        // Check if we're running on Android 5.0 or higher
                        if ((int)Build.VERSION.SdkInt < 23)
                        {
                            StartActivity(intent);
                            MAdapter.NotifyItemChanged(pos);
                        }
                        else
                        {
                            //Check to see if any permission in our group is available, if one, then all are
                            if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                                CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                StartActivity(intent);
                                MAdapter.NotifyItemChanged(pos);
                            }
                            else
                                new PermissionsController(this).RequestPermission(100);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ItemLongClick(View view, GetConversationListObject.DataConversation obj, int pos)
        {
            EnableActionMode(pos);
        }

        private void EnableActionMode(int position)
        {
            if (ActionMode == null)
            {
                ActionMode = StartSupportActionMode(ModeCallback);
            }
            ToggleSelection(position);
        }

        private void ToggleSelection(int position)
        {
            try
            {
                MAdapter.ToggleSelection(position);
                int count = MAdapter.GetSelectedItemCount();

                if (count == 0)
                {
                    HomeActivity.GetInstance()?.SetService();

                    ToolBar.Visibility = ViewStates.Visible;
                    ActionMode.Finish();
                }
                else
                {
                    HomeActivity.GetInstance()?.SetService(false);

                    ToolBar.Visibility = ViewStates.Gone;
                    ActionMode.SetTitle(count);
                    ActionMode.Invalidate();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Permissions

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 100)
                {
                    var item = MAdapter.UserList.FirstOrDefault(a => a.User.Id.ToString() == UserId);
                    if (item != null)
                    {
                        Intent intent = new Intent(this, typeof(MessagesBoxActivity));
                        intent.PutExtra("UserId", item.User.Id.ToString());
                        intent.PutExtra("UserItem", JsonConvert.SerializeObject(item));
                        StartActivity(intent);

                        MAdapter.NotifyItemChanged(MAdapter.UserList.IndexOf(item));

                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                ListUtils.ChatList.Clear();

                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();

                SqLiteDatabase database = new SqLiteDatabase();
                database.ClearLastChat();
                database.ClearAll_Messages();
                

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Api 

        private void GetLastChatLocal()
        {
            try
            {
                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                ListUtils.ChatList = new ObservableCollection<GetConversationListObject.DataConversation>();
                var list = dbDatabase.GetAllLastChat();
                if (list.Count > 0)
                {
                    ListUtils.ChatList = new ObservableCollection<GetConversationListObject.DataConversation>(list);
                    MAdapter.UserList = ListUtils.ChatList;
                    MAdapter.NotifyDataSetChanged();
                }
                else
                {
                    SwipeRefreshLayout.Refreshing = true;

                    StartApiService();
                }

                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
        }

        private async Task LoadDataAsync(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.UserList.Count;

                var (apiStatus, respond) = await RequestsAsync.Chat.GetConversationListAsync("15", offset);
                if (apiStatus != 200 || respond is not GetConversationListObject result || result.Data == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            LoadDataJsonLastChat(result);
                        }
                        else
                        {
                            ListUtils.ChatList = new ObservableCollection<GetConversationListObject.DataConversation>(result.Data);
                            MAdapter.UserList = new ObservableCollection<GetConversationListObject.DataConversation>(result.Data);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
                            
                        }
                    }
                    else
                    {
                        if (MAdapter.UserList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short)?.Show();
                    }
                }

                MainScrollEvent.IsLoading = false;
                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        public void LoadDataJsonLastChat(GetConversationListObject result)
        {
            try
            {
                if (MAdapter != null)
                {
                    if (MAdapter.UserList?.Count > 0)
                    {
                        foreach (var user in result.Data)
                        {
                            var checkUser = MAdapter.UserList.FirstOrDefault(a => a.User.Id == user.User.Id);
                            if (checkUser != null)
                            {
                                int index = MAdapter.UserList.IndexOf(checkUser);

                                //checkUser.Id = user.Id;
                                if (checkUser.Owner != user.Owner) checkUser.Owner = user.Owner;
                                if (checkUser.Time != user.Time) checkUser.Time = user.Time;
                                if (checkUser.Seen != user.Seen) checkUser.Seen = user.Seen;
                                if (checkUser.Time != user.Time) checkUser.Time = user.Time;
                                if (checkUser.CreatedAt != user.CreatedAt) checkUser.CreatedAt = user.CreatedAt;
                                if (checkUser.NewMessages != user.NewMessages) checkUser.NewMessages = user.NewMessages;
                                if (checkUser.User != user.User) checkUser.User = user.User;

                                if (checkUser.MessageType != user.MessageType) continue;
                                checkUser.MessageType = user.MessageType;

                                if (checkUser.Text != user.Text)
                                {
                                    checkUser.Text = user.Text;

                                    if (index > -1)
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            MAdapter.UserList.Move(index, 0);
                                            MAdapter.NotifyItemMoved(index, 0);
                                        });
                                    }
                                }

                                if (checkUser.Media != user.Media)
                                {
                                    checkUser.Media = user.Media;

                                    if (index > -1)
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            MAdapter.UserList.Move(index, 0);
                                            MAdapter.NotifyItemMoved(index, 0);
                                        });
                                    }
                                }

                                if (checkUser.Sticker != user.Sticker)
                                {
                                    checkUser.Sticker = user.Sticker;

                                    if (index > -1)
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            MAdapter.UserList.Move(index, 0);
                                            MAdapter.NotifyItemMoved(index, 0);
                                        });
                                    }
                                }
                            }
                            else
                            {
                                RunOnUiThread(() =>
                                {
                                    MAdapter.UserList.Insert(0, user);
                                    MAdapter.NotifyItemInserted(0);

                                    //var dataUser = MAdapter.UserList.IndexOf(MAdapter.UserList.FirstOrDefault(a => a.Id == user.Id));
                                    //if (dataUser > -1)
                                    //    MAdapter.NotifyItemChanged(dataUser);
                                });
                            }
                        }
                    }
                    else
                    {
                        MAdapter.UserList = new ObservableCollection<GetConversationListObject.DataConversation>(result.Data);
                        MAdapter.NotifyDataSetChanged();
                    }

                    ListUtils.ChatList = MAdapter.UserList;
                }

                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.UserList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoMessage);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        public override void OnBackPressed()
        {
            try
            {
                int count = MAdapter.GetSelectedItemCount();
                if (count == 0)
                {
                    base.OnBackPressed();
                }
                else
                { 
                    ToolBar.Visibility = ViewStates.Visible;
                    ActionMode.Finish();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
} 