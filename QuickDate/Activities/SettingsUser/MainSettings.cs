﻿using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using Newtonsoft.Json;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;

namespace QuickDate.Activities.SettingsUser
{
    public static class MainSettings  
    {
        public static ISharedPreferences SharedData, InAppReview;
        public static readonly string LightMode = "light";
        public static readonly string DarkMode = "dark";
        public static readonly string DefaultMode = "default";

        private const string ShowTutoralDialogKey = "SHOW_TUTORAL_DIALOG_KEY";
        private const string ShowWalkThroughPageKey = "SHOW_WALK_THROUGH_PAGE_KEY";
        private const string SwipeCountDetailsKey = "SWIPE_COUNT_DETAILS_KEY";

        public static readonly string PrefKeyInAppReview = "In_App_Review";

        public static void Init()
        {
            try
            {
                SharedData = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                InAppReview = Application.Context.GetSharedPreferences("In_App_Review", FileCreationMode.Private);

                AppSettings.ShowWalkTroutPage = GetShowWalkThroughPageValue();

                string getValue = SharedData.GetString("Night_Mode_key", string.Empty);
                ApplyTheme(getValue); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static void ApplyTheme(string themePref)
        {
            try
            {
                if (themePref == LightMode)
                {
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                    AppSettings.SetTabDarkTheme = false;
                }
                else if (themePref == DarkMode)
                {
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                    AppSettings.SetTabDarkTheme = true;
                }
                else if (themePref == DefaultMode)
                {
                    AppCompatDelegate.DefaultNightMode = (int)Build.VERSION.SdkInt >= 29 ? AppCompatDelegate.ModeNightFollowSystem : AppCompatDelegate.ModeNightAutoBattery;

                    var currentNightMode = Application.Context.Resources?.Configuration?.UiMode & UiMode.NightMask;
                    switch (currentNightMode)
                    {
                        case UiMode.NightNo:
                            // Night mode is not active, we're using the light theme
                            AppSettings.SetTabDarkTheme = false;
                            break;
                        case UiMode.NightYes:
                            // Night mode is active, we're using dark theme
                            AppSettings.SetTabDarkTheme = true;
                            break;
                    }
                }
                else
                {
                    if (AppSettings.SetTabDarkTheme) return;

                    var currentNightMode = Application.Context.Resources?.Configuration?.UiMode & UiMode.NightMask;
                    switch (currentNightMode)
                    {
                        case UiMode.NightNo:
                            // Night mode is not active, we're using the light theme
                            AppSettings.SetTabDarkTheme = false;
                            break;
                        case UiMode.NightYes:
                            // Night mode is active, we're using dark theme
                            AppSettings.SetTabDarkTheme = true;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void StoreShowTutorialDialogValue(bool showTutorialDialogAgain)
        {
            try
            {
                SharedData?.Edit()?.PutBoolean(ShowTutoralDialogKey, showTutorialDialogAgain)?.Commit();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
            
        }

        public static bool GetShowTutorialDialogValue()
        {
            try
            {
                return SharedData.GetBoolean(ShowTutoralDialogKey, true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;
            } 
        }

        public static void StoreShowWalkThroughPageValue(bool showWalkThroughPageAgain)
        {
            try
            {
                SharedData?.Edit()?.PutBoolean(ShowWalkThroughPageKey, showWalkThroughPageAgain)?.Commit();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            } 
        }

        public static bool GetShowWalkThroughPageValue()
        {
            try
            {
                return SharedData.GetBoolean(ShowWalkThroughPageKey, true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;
            } 
        }

        public static void StoreSwipeCountValue(int swipeCount)
        {
            try
            {
                var swipeDetail = new SwipeLimitDetails
                {
                    SwapCount = swipeCount,
                    LastSwapDate = DateTime.UtcNow
                };
                var swipeDetailAsJsonString = JsonConvert.SerializeObject(swipeDetail);

                SharedData?.Edit()?.PutString(SwipeCountDetailsKey, swipeDetailAsJsonString)?.Commit();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            } 
        }

        public static bool CanSwipeMore(int maxSwapLimit)
        {
            try
            {
                var swipeDetailJsonString = SharedData.GetString(SwipeCountDetailsKey, null);

                if (swipeDetailJsonString == null)
                {
                    return true;
                }

                var swipeDetail = JsonConvert.DeserializeObject<SwipeLimitDetails>(swipeDetailJsonString);

                return swipeDetail.CanSwipe(maxSwapLimit);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;
            } 
        }

        public static int GetSwipeCountValue()
        {
            try
            {
                var swipeDetailJsonString = SharedData.GetString(SwipeCountDetailsKey, null);

                if (swipeDetailJsonString == null)
                {
                    return 0;
                }

                var swipeDetail = JsonConvert.DeserializeObject<SwipeLimitDetails>(swipeDetailJsonString);

                return swipeDetail.GetSwipeCount();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }
    }
}
