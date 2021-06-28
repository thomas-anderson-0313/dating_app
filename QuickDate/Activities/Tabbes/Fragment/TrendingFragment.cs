using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Plugin.Geolocator;
using QuickDate.Activities.HotOrNot;
using QuickDate.Activities.SearchFilter;
using QuickDate.Activities.Tabbes.Adapters;
using QuickDate.ButtomSheets;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDateClient.Classes.Global;
using QuickDateClient.Classes.Users;
using QuickDateClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace QuickDate.Activities.Tabbes.Fragment
{
    public class TrendingFragment : AndroidX.Fragment.App.Fragment 
    {
        #region Variables Basic

        public TrendingAdapter MAdapter;
        public SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private GridLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        public RecyclerViewOnScrollListener MainScrollEvent;
        private HomeActivity GlobalContext; 
        private TextView ToolbarTitle;
        private ImageView FilterButton;
        private LocationManager LocationManager;
        private bool ShowAlertDialogGps = true;
        private int CountOffset; 
        public HotOrNotFragment HotOrNotFragment;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            GlobalContext = (HomeActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // Use this to return your custom view for this Fragment
                View view = inflater.Inflate(Resource.Layout.TTrendingLayout, container, false);  
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                InitializeLocationManager();

                //Get Data Api
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);

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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                FilterButton = view.FindViewById<ImageView>(Resource.Id.Filterbutton);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
                ToolbarTitle = view.FindViewById<TextView>(Resource.Id.toolbartitle);
                ToolbarTitle.Text = AppSettings.ApplicationName;

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
              
                
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                FilterButton.Click += FilterButtonOnClick; 
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
                MAdapter = new TrendingAdapter(Activity)
                {
                    TrendingList = new ObservableCollection<Classes.TrendingClass>()
                };
                LayoutManager = new GridLayoutManager(Context , 2);
                LayoutManager.SetSpanSizeLookup(new MySpanSizeLookup2(2, 1, 2));//20, 1, 4
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter);

                MAdapter.UsersItemClick += MAdapterOnUsersItemClick;
                MAdapter.UsersLikeButtonItemClick += MAdapterOnUsersLikeButtonItemClick;

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
          
        private void InitToolbar(View view)
        {
            try
            {
                var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    GlobalContext.SetToolBar(toolbar, "", false, false);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Open search filter
        private void FilterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (AppSettings.ShowFilterBasic && !AppSettings.ShowFilterLooks && !AppSettings.ShowFilterBackground && !AppSettings.ShowFilterLifestyle && !AppSettings.ShowFilterMore)
                {
                    var searchFilter = new SearchFilterBottomDialogFragment();
                    searchFilter.Show(Activity.SupportFragmentManager, "searchFilter");
                }
                else
                {
                    Context.StartActivity(new Intent(Context, typeof(SearchFilterTabbedActivity)));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open profile user >> Near By
        private void MAdapterOnUsersItemClick(object sender, NearByAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;

                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    QuickDateTools.OpenProfile(Activity, "LikeAndMoveTrending", item.UsersData, e.Image);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Add Like >> Near By
        private void MAdapterOnUsersLikeButtonItemClick(object sender, NearByAdapterClickEventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                if (e.Position <= -1) return;

                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (item.UsersData.IsLiked != null && item.UsersData.IsLiked.Value)
                    {
                        item.UsersData.IsLiked = false;
                        //sent api 
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Users.DeleteLikesAsync(item.UsersData.Id.ToString()) });
                    }
                    else
                    {
                        item.UsersData.IsLiked = true;
                        //sent api 
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Users.AddLikesAsync(item.UsersData.Id.ToString(), "") });
                    }

                    MAdapter.NotifyItemChanged(e.Position);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                //Get Data Api
                MAdapter.TrendingList.Clear();
                MAdapter.NotifyDataSetChanged();

                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Scroll

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    return;

                //Code get last id where LoadMore >>
                var checkList = MAdapter.TrendingList.FirstOrDefault(q => q.Type == ItemType.Users);
                if (MainScrollEvent != null && checkList != null && !MainScrollEvent.IsLoading)
                {
                    CountOffset += 1;
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadUsersAsync(CountOffset.ToString()) });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Api 

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadProUser });
        }

        private async Task LoadProUser()
        { 
            if (Methods.CheckConnectivity())
            {
                //int countList = MAdapter.TrendingList.Count;
                var (apiStatus, respond) = await RequestsAsync.Users.GetProAsync("20");
                if (apiStatus != 200 || respond is not ListUsersObject result || result.Data == null)
                {
                    if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        var checkList = MAdapter.TrendingList.FirstOrDefault(q => q.Type == ItemType.ProUser);
                        if (checkList == null)
                        {
                            var proUser = new Classes.TrendingClass
                            {
                                Id = 100,
                                ProUserList = new List<UserInfoObject>(),
                                Type = ItemType.ProUser
                            };

                            var data = ListUtils.MyUserInfo?.FirstOrDefault();
                            if (data?.IsPro != "1")
                            {
                                if (!AppSettings.EnableAppFree)
                                {
                                    var dataOwner = proUser.ProUserList.FirstOrDefault(a => a.Type == "Your");
                                    if (dataOwner == null && data != null)
                                    {
                                        data.Type = "Your";
                                        data.Username = Context.GetText(Resource.String.Lbl_AddMe);
                                        data.IsOwner = true;
                                        proUser.ProUserList.Insert(0, data); 
                                    }
                                }
                            }

                            foreach (var item in from item in result.Data let check = proUser.ProUserList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                proUser.ProUserList.Add(item);
                            }

                            MAdapter.TrendingList.Insert(0,proUser);
                        }
                        else
                        {
                            foreach (var item in from item in result.Data let check = checkList.ProUserList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                checkList.ProUserList.Add(item);
                            }
                        }
                    }

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadHotOrNotAsync() });
                } 
            }
            else
            {
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private async Task LoadHotOrNotAsync(string offset = "0")
        { 
            if (Methods.CheckConnectivity())
            { 
                //int countList = MAdapter.TrendingList.Count;
                var (apiStatus, respond) = await RequestsAsync.Users.HotOrNotAsync(UserDetails.Gender, "20", offset);
                if (apiStatus != 200 || respond is not ListUsersObject result || result.Data == null)
                {
                    if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        var checkList = MAdapter.TrendingList.FirstOrDefault(q => q.Type == ItemType.HotOrNot);
                        if (checkList == null)
                        {
                            var hotOrNot = new Classes.TrendingClass
                            {
                                Id = 200,
                                HotOrNotList = new List<UserInfoObject>(),
                                Type = ItemType.HotOrNot
                            };

                            foreach (var item in from item in result.Data let check = hotOrNot.HotOrNotList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                hotOrNot.HotOrNotList.Add(item);
                            }

                            MAdapter.TrendingList.Insert(1, hotOrNot);
                            Activity?.RunOnUiThread(() => { MAdapter.NotifyItemInserted(1); });
                        }
                        else
                        {
                            foreach (var item in from item in result.Data let check = checkList.HotOrNotList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                checkList.HotOrNotList.Add(item);
                            }
                        }
                    } 
                }

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadUser });
            }
            else
            {
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        public async Task LoadUser()
        {
            // Check if we're running on Android 5.0 or higher
            if ((int)Build.VERSION.SdkInt < 23)
                await GetPosition();
            else
            {
                if (Context.CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && Context.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                    await GetPosition();
                else
                    new PermissionsController(Activity).RequestPermission(105);
            }
        }

        private async Task LoadUsersAsync(string offset = "0")
        {
            if (MainScrollEvent != null && MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                if (MainScrollEvent != null)
                    MainScrollEvent.IsLoading = true;

                if (UserDetails.Lat == "" && UserDetails.Lng == "")
                {
                    var locator = CrossGeolocator.Current;
                    locator.DesiredAccuracy = 50;
                    var position = await locator.GetPositionAsync(TimeSpan.FromMilliseconds(10000));
                    Console.WriteLine("Position Status: {0}", position.Timestamp);
                    Console.WriteLine("Position Latitude: {0}", position.Latitude);
                    Console.WriteLine("Position Longitude: {0}", position.Longitude);

                    UserDetails.Lat = position.Latitude.ToString(CultureInfo.InvariantCulture);
                    UserDetails.Lng = position.Longitude.ToString(CultureInfo.InvariantCulture);
                }

                UserDetails.Location = ListUtils.MyUserInfo?.FirstOrDefault()?.Location;

                var dictionary = new Dictionary<string, string>
                {
                    {"limit", "25"},
                    {"offset", offset},
                    {"_gender", UserDetails.Gender},
                    {"_located", UserDetails.Located},
                    {"_location", UserDetails.Location},
                    {"_age_from", UserDetails.AgeMin.ToString()},
                    {"_age_to",  UserDetails.AgeMax.ToString()},
                    {"_lat", UserDetails.Lat},
                    {"_lng", UserDetails.Lng},
                    {"_body", UserDetails.Body ?? ""},
                    {"_ethnicity", UserDetails.Ethnicity ?? ""},
                    {"_religion", UserDetails.Religion ?? ""},
                    {"_drink", UserDetails.Drink ?? ""},
                    {"_smoke", UserDetails.Smoke ?? ""},
                    {"_education", UserDetails.Education ?? ""},
                    {"_pets", UserDetails.Pets ?? ""},
                    {"_relationship", UserDetails.RelationShip ?? ""},
                    {"_language", UserDetails.Language ?? UserDetails.FilterOptionLanguage},
                    {"_interest", UserDetails.Interest ?? ""},
                    {"_height_from", UserDetails.FromHeight ?? UserDetails.FilterOptionFromHeight},
                    {"_height_to", UserDetails.ToHeight ?? UserDetails.FilterOptionToHeight},
                };

                var (apiStatus, respond) = await RequestsAsync.Users.SearchAsync(dictionary);
                if (apiStatus != 200 || respond is not ListUsersObject result || result.Data == null)
                {
                    if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in result.Data)
                        {
                            var check = MAdapter.TrendingList.FirstOrDefault(a => a.Id == item.Id);
                            if (check == null)
                            {
                                var users = new Classes.TrendingClass
                                {
                                    Id = item.Id,
                                    UsersData = item,
                                    Type = ItemType.Users
                                };

                                if (UserDetails.SwitchState)
                                {
                                    var online = QuickDateTools.GetStatusOnline(item.Lastseen, item.Online);
                                    if (!online) continue;
                                    MAdapter.TrendingList.Add(users);
                                }
                                else
                                {
                                    MAdapter.TrendingList.Add(users);
                                }
                            }
                        }
                    } 
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
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

                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
            }
            if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
        }
         
        private void ShowEmptyPage()
        {
            try
            {
                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.TrendingList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    var emptyStateChecker = MAdapter.TrendingList.FirstOrDefault(a => a.Type == ItemType.EmptyPage);
                    if (emptyStateChecker != null && MAdapter.TrendingList.Count > 1)
                    {
                        MAdapter.TrendingList.Remove(emptyStateChecker);
                    }
                     
                    MAdapter.NotifyDataSetChanged();
                }
                else
                {
                    var checkList = MAdapter.TrendingList.FirstOrDefault(q => q.Type == ItemType.Users);
                    if (checkList == null)
                    {
                        MAdapter.TrendingList.Add(new Classes.TrendingClass
                        {
                            Id = 400,
                            Type = ItemType.EmptyPage
                        });
                        MAdapter.NotifyDataSetChanged();
                    }
                }
            }
            catch (Exception e)
            {
                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
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
             
        #region Location

        private void InitializeLocationManager()
        {
            try
            {
                LocationManager = (LocationManager)Activity.GetSystemService(Context.LocationService);
                var criteriaForLocationService = new Criteria
                {
                    Accuracy = Accuracy.Fine
                };
                var acceptableLocationProviders = LocationManager.GetProviders(criteriaForLocationService, true);
                var locationProvider = acceptableLocationProviders.Any() ? acceptableLocationProviders.First() : string.Empty;
                Console.WriteLine(locationProvider);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Position GPS Current Location
        private async Task GetPosition()
        {
            try
            {
                if (CrossGeolocator.Current.IsGeolocationAvailable)
                {
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        CheckAndGetLocation();
                    }
                    else
                    {
                        if (Context.CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && Context.CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                        {
                            CheckAndGetLocation();
                        }
                        else
                        {
                            new PermissionsController(Activity).RequestPermission(105);
                        }
                    }
                }

                await Task.Delay(0);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void CheckAndGetLocation()
        {
            try
            {
                Activity?.RunOnUiThread(async () =>
                {
                    try
                    {
                        if (!LocationManager.IsProviderEnabled(LocationManager.GpsProvider))
                        {
                            if (ShowAlertDialogGps)
                            {
                                ShowAlertDialogGps = false;

                                Activity?.RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        // Call your Alert message
                                        AlertDialog.Builder alert = new AlertDialog.Builder(Context);
                                        alert.SetTitle(GetString(Resource.String.Lbl_Use_Location) + "?");
                                        alert.SetMessage(GetString(Resource.String.Lbl_GPS_is_disabled) + "?");

                                        alert.SetPositiveButton(GetString(Resource.String.Lbl_Ok), (senderAlert, args) =>
                                        {
                                            //Open intent Gps
                                            new IntentController(Activity).OpenIntentGps(LocationManager);
                                        });

                                        alert.SetNegativeButton(GetString(Resource.String.Lbl_Cancel), (senderAlert, args) => { });

                                        Dialog gpsDialog = alert.Create();
                                        gpsDialog.Show();
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                        else
                        {
                            var locator = CrossGeolocator.Current;
                            locator.DesiredAccuracy = 50;
                            var position = await locator.GetPositionAsync(TimeSpan.FromMilliseconds(10000));
                            Console.WriteLine("Position Status: {0}", position.Timestamp);
                            Console.WriteLine("Position Latitude: {0}", position.Latitude);
                            Console.WriteLine("Position Longitude: {0}", position.Longitude);

                            UserDetails.Lat = position.Latitude.ToString(CultureInfo.InvariantCulture);
                            UserDetails.Lng = position.Longitude.ToString(CultureInfo.InvariantCulture);

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadUsersAsync() });
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion 
    }
}