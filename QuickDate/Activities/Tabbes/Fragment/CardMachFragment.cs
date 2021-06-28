using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AT.Markushi.UI;
using Com.Yuyakaido.Android.CardStackView;
using Java.Lang;
using ME.Alexrs.Wavedrawable;
using QuickDate.Activities.Premium;
using QuickDate.Activities.SettingsUser;
using QuickDate.Activities.Tabbes.Adapters;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDateClient.Classes.Users;
using QuickDateClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace QuickDate.Activities.Tabbes.Fragment
{
    public class CardMachFragment : AndroidX.Fragment.App.Fragment, CardStackView.ICardEventListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private CardStackView CardStack;
        public CardAdapter CardDateAdapter;
        private CircleButton LikeButton, DesLikeButton, UndoButton;
        private HomeActivity GlobalContext;
        private WaveDrawable WaveDrawableAnimation;
        private ImageView ImageView, FilterButton;
        private ImageView PopularityImage;
        private RelativeLayout CardViewBig;
        private LinearLayout BtnLayout;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private int TotalCount, SwipeCount;
        private string TotalIdLiked = "", TotalIdDisLiked = "", IdGender = UserDetails.FilterOptionGenderMale;
        private SwipeDirection Direction;
        private int Index;
        public Handler MainHandler = new Handler(Looper.MainLooper);
        public IRunnable Runnable;
        private const int PostUpdaterInterval = 50000; 

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
            
            SwipeCount = MainSettings.GetSwipeCountValue();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
               var view = inflater.Inflate(Resource.Layout.TCardMachLayout, container, false);
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

                InitComponent(View);
                InitToolbar(View);
                SetRecyclerViewAdapters();

                var doubleClickHelper = new DoubleClickHandler(500);

                LikeButton.Click += (s, e) => doubleClickHelper.HandleDoubleClick(LikeButtonOnClick);
                DesLikeButton.Click += (s, e) => doubleClickHelper.HandleDoubleClick(DesLikeButtonOnClick);
                UndoButton.Click += (s, e) => doubleClickHelper.HandleDoubleClick(UndoButtonOnClick);

                PopularityImage.Click += PopularityImageOnClick;
                FilterButton.Click += FilterButtonOnClick;

                StartApiService();

                Runnable = new PostUpdaterHelper(GlobalContext, MainHandler);
                //Start Updating the news feed every few minus 
                MainHandler.PostDelayed(Runnable, PostUpdaterInterval);
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                CardStack = view.FindViewById<CardStackView>(Resource.Id.activity_main_card_stack_view);
                LikeButton = view.FindViewById<CircleButton>(Resource.Id.likebutton2);
                DesLikeButton = view.FindViewById<CircleButton>(Resource.Id.closebutton1);
                UndoButton = view.FindViewById<CircleButton>(Resource.Id.Undobutton1);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
                PopularityImage = view.FindViewById<ImageView>(Resource.Id.coinImage);
                CardViewBig = view.FindViewById<RelativeLayout>(Resource.Id.CardViewBig);
                FilterButton = view.FindViewById<ImageView>(Resource.Id.Filterbutton);

                CardStack.SetCardEventListener(this);
                List<SwipeDirection> direction = new List<SwipeDirection> { SwipeDirection.Right, SwipeDirection.Left };
                CardStack.SetSwipeDirection(direction);
                CardStack.SetStackFrom(StackFrom.Top);
                CardStack.SetVisibleCount(3);
                CardStack.SetTranslationDiff(8.0f);

                BtnLayout = view.FindViewById<LinearLayout>(Resource.Id.buttonLayout);
                ImageView = view.FindViewById<ImageView>(Resource.Id.userImageView);

                WaveDrawableAnimation = new WaveDrawable(Color.ParseColor(AppSettings.MainColor), 800);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
                    ImageView.Background = WaveDrawableAnimation;
                WaveDrawableAnimation.SetWaveInterpolator(new LinearInterpolator());
                WaveDrawableAnimation.StartAnimation();

                ImageView.Visibility = ViewStates.Visible;
                CardStack.Visibility = ViewStates.Invisible;
                BtnLayout.Visibility = ViewStates.Invisible;
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                CardDateAdapter = new CardAdapter(Activity);
                CardStack.SetAdapter(CardDateAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Get User By >> gender
        private void FilterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var genderArray = ListUtils.SettingsSiteList?.Gender;

                if (genderArray?.Count > 0)
                {
                    var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    var arrayAdapter = genderArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())).ToList();
                    arrayAdapter.Insert(0, GetString(Resource.String.Lbl_Default));

                    dialogList.Title(GetText(Resource.String.Lbl_Gender));
                    dialogList.Items(arrayAdapter);
                    dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Methods.DisplayReportResult(Activity, "List Gender Not Found, Please check api option ");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PopularityImageOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Context, typeof(PopularityActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UndoButtonOnClick()
        {
            try
            {
                var data = ListUtils.OldMatchesList.LastOrDefault();
                if (data != null)
                {
                    CardDateAdapter.UsersDateList.Insert(0, data);
                    CardDateAdapter.NotifyDataSetChanged();

                    ListUtils.OldMatchesList.Remove(data);
                }

                CardStack.Reverse();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DesLikeButtonOnClick()
        {
            try
            {
                SetDesLikeDirection();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikeButtonOnClick()
        {
            try
            {
                SetLikeDirection();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void SetLikeDirection()
        {
            try
            {
                var maxSwaps = ListUtils.SettingsSiteList?.MaxSwaps;
                var isPro = ListUtils.MyUserInfo?.FirstOrDefault()?.IsPro ?? "0";
                if (isPro == "0" && SwipeCount == Convert.ToInt32(maxSwaps))
                {
                    //You have exceeded the maximum of likes or swipes per this day
                    var window = new DialogController(Activity);
                    window.OpenDialogGetToPremium();

                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_ErrorMaxSwaps), ToastLength.Short)?.Show();
                    //DialogGoToPremium
                    CardStack.SetSwipeDirection(new List<SwipeDirection>());
                    return;
                }

                ValueAnimator rotation = ObjectAnimator.OfPropertyValuesHolder(CardStack.TopView, PropertyValuesHolder.OfFloat("rotation", 10f));
                rotation.SetDuration(200);
                ValueAnimator translateX = ObjectAnimator.OfPropertyValuesHolder(CardStack.TopView, PropertyValuesHolder.OfFloat("translationX", 0f, 2000f));
                ValueAnimator translateY = ObjectAnimator.OfPropertyValuesHolder(CardStack.TopView, PropertyValuesHolder.OfFloat("translationY", 0f, -500f));

                translateX.StartDelay = 100;
                translateY.StartDelay = 100;
                translateX.SetDuration(500);
                translateY.SetDuration(500);

                AnimatorSet cardAnimationSet = new AnimatorSet();
                cardAnimationSet.PlayTogether(rotation, translateX, translateY);

                ObjectAnimator overlayAnimator = ObjectAnimator.OfFloat(CardStack.TopView.OverlayContainer, "alpha", 0f, 1f);
                overlayAnimator.SetDuration(200);
                AnimatorSet overlayAnimationSet = new AnimatorSet();
                overlayAnimationSet.PlayTogether(overlayAnimator);

                CardStack.Swipe(SwipeDirection.Right, overlayAnimationSet);

                //int index = CardStack.TopIndex - 1;
                ////CardContainerView view = CardStack.BottomView;

                //if (index > -1)
                //    CardAppeared(index);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetDesLikeDirection()
        {
            try
            {
                var maxSwaps = ListUtils.SettingsSiteList?.MaxSwaps;
                var isPro = ListUtils.MyUserInfo?.FirstOrDefault()?.IsPro ?? "0";
                if (isPro == "0" && SwipeCount == Convert.ToInt32(maxSwaps))
                {
                    //You have exceeded the maximum of likes or swipes per this day
                    var window = new DialogController(Activity);
                    window.OpenDialogGetToPremium();

                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_ErrorMaxSwaps), ToastLength.Short)?.Show();
                    //DialogGoToPremium
                    CardStack.SetSwipeDirection(new List<SwipeDirection>());
                    return;
                }

                ValueAnimator rotation = ObjectAnimator.OfPropertyValuesHolder(CardStack.TopView, PropertyValuesHolder.OfFloat("rotation", -10f));
                rotation.SetDuration(200);
                ValueAnimator translateX = ObjectAnimator.OfPropertyValuesHolder(CardStack.TopView, PropertyValuesHolder.OfFloat("translationX", 0f, -2000f));
                ValueAnimator translateY = ObjectAnimator.OfPropertyValuesHolder(CardStack.TopView, PropertyValuesHolder.OfFloat("translationY", 0f, -500f));

                translateX.StartDelay = 100;
                translateY.StartDelay = 100;
                translateX.SetDuration(500);
                translateY.SetDuration(500);

                AnimatorSet cardAnimationSet = new AnimatorSet();
                cardAnimationSet.PlayTogether(rotation, translateX, translateY);

                ObjectAnimator overlayAnimator = ObjectAnimator.OfFloat(CardStack.TopView.OverlayContainer, "alpha", 0f, 1f);
                overlayAnimator.SetDuration(200);
                AnimatorSet overlayAnimationSet = new AnimatorSet();
                overlayAnimationSet.PlayTogether(overlayAnimator);

                CardStack.Swipe(SwipeDirection.Left, overlayAnimationSet);

                //int index = CardStack.TopIndex - 1;
                ////CardContainerView view = CardStack.BottomView;
                //if (index > -1)
                //    CardDisappeared(index);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SaveSwipeCount()
        {
            MainSettings.StoreSwipeCountValue(SwipeCount);
        }

        #endregion

        #region CardEventListener

        public void OnCardClicked(int index)
        {
            try
            {
                //var data = CardDateAdapter.UsersDateList[index];
                //if (data != null)
                //{
                //    index += 1; 
                //    var nextDataUser = CardDateAdapter.UsersDateList[index];

                //    QuickDateTools.OpenProfile(Activity, "LikeAndMoveCardMach", nextDataUser, null);
                //}
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnCardDragging(float percentX, float percentY)
        {

        }

        public void OnCardMovedToOrigin()
        {

        }

        public void OnCardReversed()
        {

        }

        public void OnCardSwiped(SwipeDirection direction)
        {
            try
            {
                Index = CardStack.TopIndex - 1;
                Direction = direction;
                new Handler(Looper.MainLooper).Post(new Runnable(Run));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Run()
        {
            try
            {
                if (Direction == SwipeDirection.Right)
                {
                    SwipeCount++;
                    CardAppeared(Index);
                    GlobalContext?.TracksCounter?.CheckTracksCounter();
                }
                else if (Direction == SwipeDirection.Left)
                {
                    SwipeCount++;
                    CardDisappeared(Index);
                    GlobalContext?.TracksCounter?.CheckTracksCounter();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CardAppeared(int position)
        {
            try
            {
                if (CardDateAdapter.UsersDateList.Count > 0)
                {
                    if (position == -1)
                        position = 0;

                    if (position >= 0)
                    {
                        //var textView = view.FindViewById<TextView>(Resource.Id.item_tourist_spot_card_name);
                        //if (textView != null)
                        //{
                        //    string name = textView.Text;
                        //}

                        if (position == CardDateAdapter.UsersDateList.Count)
                            position = CardDateAdapter.UsersDateList.Count - 1;

                        var data = CardDateAdapter.UsersDateList[position];

                        if (data != null)
                        {
                            if (data.IsLiked != null && data.IsLiked.Value)
                            {
                                Activity?.RunOnUiThread(() =>
                                {
                                    new DialogController(Activity).OpenDialogMatchFound(data);
                                });
                            }

                            ListUtils.LikedList.Add(data);
                            ListUtils.OldMatchesList.Add(data);

                            Activity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    CardDateAdapter.UsersDateList.Remove(data);
                                    CardDateAdapter.NotifyDataSetChanged();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });

                            TotalCount += 1;
                        }

                        CheckerCountCard();
                    }
                }
                else
                {
                    CheckerCountCard();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CardDisappeared(int position)
        {
            try
            {
                if (CardDateAdapter.UsersDateList.Count > 0)
                {
                    if (position == -1)
                        position = 0;

                    if (position >= 0)
                    {
                        if (position == CardDateAdapter.UsersDateList.Count)
                            position = CardDateAdapter.UsersDateList.Count - 1;

                        var data = CardDateAdapter.UsersDateList[position];
                        if (data != null)
                        {
                            ListUtils.DisLikedList.Add(data);
                            ListUtils.OldMatchesList.Add(data);
                            TotalCount += 1;

                            Activity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    CardDateAdapter.UsersDateList.Remove(data);
                                    CardDateAdapter.NotifyDataSetChanged();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                        }
                        CheckerCountCard();
                    }
                    else
                    {
                        CheckerCountCard();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CheckerCountCard()
        {
            try
            {
                if (TotalCount >= 6 || CardDateAdapter.UsersDateList.Count == 0)
                {
                    if (ListUtils.LikedList.Count > 0)
                    {
                        TotalIdLiked = "";
                        //Get all id 
                        foreach (var item in ListUtils.LikedList)
                        {
                            TotalIdLiked += item.Id + ",";
                        }
                    }

                    if (ListUtils.DisLikedList.Count > 0)
                    {
                        TotalIdDisLiked = "";
                        //Get all id 
                        foreach (var item in ListUtils.DisLikedList)
                        {
                            TotalIdDisLiked += item.Id + ",";
                        }
                    }

                    if (!string.IsNullOrEmpty(TotalIdLiked))
                        TotalIdLiked = TotalIdLiked.Remove(TotalIdLiked.Length - 1, 1);

                    if (!string.IsNullOrEmpty(TotalIdDisLiked))
                        TotalIdDisLiked = TotalIdDisLiked.Remove(TotalIdDisLiked.Length - 1, 1);

                    if (!string.IsNullOrEmpty(TotalIdDisLiked) || !string.IsNullOrEmpty(TotalIdDisLiked)) //sent api 
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Users.AddLikesAsync(TotalIdLiked, TotalIdDisLiked) });

                    TotalCount = 0;
                    ListUtils.LikedList.Clear();
                    ListUtils.DisLikedList.Clear();
                    TotalIdDisLiked = "";
                    TotalIdLiked = "";
                }

                //Load More
                int count = CardDateAdapter.UsersDateList.Count;
                if (count <= 5)
                {
                    var offset = CardDateAdapter.UsersDateList.LastOrDefault()?.Id ?? 0;
                    StartApiService(offset.ToString());
                }

                var maxSwaps = ListUtils.SettingsSiteList?.MaxSwaps;
                var isPro = ListUtils.MyUserInfo?.FirstOrDefault()?.IsPro ?? "0";
                if (isPro == "0" && SwipeCount == Convert.ToInt32(maxSwaps))
                {
                    //You have exceeded the maximum of likes or swipes per this day
                    var window = new DialogController(Activity);
                    window.OpenDialogGetToPremium();

                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_ErrorMaxSwaps), ToastLength.Short)?.Show();
                    //DialogGoToPremium
                    CardStack.SetSwipeDirection(new List<SwipeDirection>());
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Random Users

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
            {
                ImageView.Visibility = ViewStates.Gone;
                CardStack.Visibility = ViewStates.Gone;
                BtnLayout.Visibility = ViewStates.Gone;

                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadMatches(offset) });
        }

        private async Task LoadMatches(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Users.GetRandomUsersAsync(IdGender, "30", offset);
                if (apiStatus != 200 || respond is not ListUsersObject result || result.Data == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    if (result.Data?.Count > 0)
                    {
                        foreach (var item in from item in result.Data let data = ListUtils.AllMatchesList.FirstOrDefault(a => a.Id == item.Id) where data == null select item)
                        {
                            CardDateAdapter.UsersDateList.Add(item);

                            ListUtils.AllMatchesList.Add(item);
                        }

                        Activity?.RunOnUiThread(() => { CardDateAdapter.NotifyDataSetChanged(); });
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(offset))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Long)?.Show();
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);

                // Open Dialog Tutorial
                OpenDialog();
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
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                if (CardDateAdapter.UsersDateList.Count > 0)
                {
                    CardDateAdapter.NotifyDataSetChanged();

                    ImageView.Visibility = ViewStates.Gone;
                    CardStack.Visibility = ViewStates.Visible;
                    BtnLayout.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    if (WaveDrawableAnimation.IsAnimationRunning)
                        WaveDrawableAnimation?.StopAnimation();
                }
                else
                {
                    ImageView.Visibility = ViewStates.Gone;
                    CardStack.Visibility = ViewStates.Gone;
                    BtnLayout.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoMatches);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }

                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
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

        private void OpenDialog()
        {
            try
            {
                var showTutorialDialog = MainSettings.GetShowTutorialDialogValue();

                if (showTutorialDialog)
                {
                    new DialogController(Activity).OpenDialogSkipTutorial();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class PostUpdaterHelper : Java.Lang.Object, IRunnable
        {
            private readonly Handler MainHandler;
            private readonly HomeActivity Activity;

            public PostUpdaterHelper(HomeActivity activity, Handler mainHandler)
            {
                MainHandler = mainHandler;
                Activity = activity;
            }

            public void Run()
            {
                try
                {
                    HomeActivity.GetInstance()?.GetNotifications();
                    //ApiRequest.GetInfoData(Activity, UserDetails.UserId.ToString()).ConfigureAwait(false);
                    MainHandler?.PostDelayed(new PostUpdaterHelper(Activity, new Handler(Looper.MainLooper)), 20000);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                var genderArray = ListUtils.SettingsSiteList?.Gender?.FirstOrDefault(a => a.ContainsValue(itemString.ToString()))?.Keys.FirstOrDefault();
                if (itemString.ToString() == GetString(Resource.String.Lbl_Default))
                {
                    var enumerable = ListUtils.SettingsSiteList?.Gender;
                    if (enumerable != null)
                    {
                        IdGender = "";
                        foreach (var id in enumerable.Select(upper => upper.Keys.FirstOrDefault()))
                        {
                            IdGender += id + ",";
                        }

                        IdGender = !string.IsNullOrEmpty(IdGender) ? IdGender.Remove(IdGender.Length - 1, 1) : UserDetails.FilterOptionGenderMale;
                    }
                }
                else
                {
                    IdGender = genderArray ?? UserDetails.FilterOptionGenderMale;
                }

                CardDateAdapter.UsersDateList.Clear();
                ListUtils.AllMatchesList.Clear();
                CardDateAdapter.NotifyDataSetChanged();

                WaveDrawableAnimation.StartAnimation();

                ImageView.Visibility = ViewStates.Visible;
                CardStack.Visibility = ViewStates.Invisible;
                BtnLayout.Visibility = ViewStates.Invisible;
                EmptyStateLayout.Visibility = ViewStates.Gone;

                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}