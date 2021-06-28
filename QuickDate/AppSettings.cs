//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PixelPhoto 15/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================
//For the accuracy of the icon and logo, please use this website " http://nsimage.brosteins.com " and add images according to size in folders " mipmap " 

using Android.Graphics;
using QuickDate.Helpers.Model;

namespace QuickDate
{
    internal static class AppSettings
    {
        //Main Settings >>>>>
        //*********************************************************
        public static string TripleDesAppServiceProvider = "DuSHyLk1edOMFoNGosK5f5wLx6V5n5/TFo7og1hlTg47bCjCCjFdOW+HjyAJ2f2wsGDTeC5YFM6duUm9YiQqjoHEUb06t7//wwGEi+QUEPRzSLN2y9p4qhHFZWEekQ7Tz+NQZy7DFwr8jXOEafnL2fTO6bTPCu1u9Komn/sbPcQ6Ssh4AYQc4NprdeyysN8kYoEpk16IxWiSyUHyXve7LJ4+5zbzhDxvBDTFrGxTx10=";
    
        public static string Version = "1.9";
        public static string ApplicationName = "QuickDate";
        public static string DatabaseName = "QuickDate"; 

        //Main Colors >>
        //*********************************************************
        public static string MainColor = "#a33596"; 
        public static Color TitleTextColor = Color.Black;
        public static Color TitleTextColorDark = Color.White;

        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = true;
        public static string Lang = ""; //Default language ar

        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "0eeb44be-0ee2-422c-99b7-d338c59c5906"; 

        //********************************************************* 

        //Add Animation Image User
        //*********************************************************
        public static bool EnableAddAnimationImageUser = false;
         
        //Set Theme Full Screen App
        //*********************************************************
        public static bool EnableFullScreenApp = false;

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file or AndroidManifest.xml
        //Facebook >> ../values/analytic.xml  
        //Google >> ../Properties/AndroidManifest.xml .. line 42
        //*********************************************************
        public static bool ShowFacebookLogin = true;
        public static bool ShowGoogleLogin = true; 
        public static bool ShowWoWonderLogin = true;  
        public static bool ShowSocialLoginAtRegisterScreen = true;
         
        public static string ClientId = "716215768781-1riglii0rihhc9gmp53qad69tt8o2e03.apps.googleusercontent.com";

        public static string AppNameWoWonder = "WoWonder"; 

        //AdMob >> Please add the code ads in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowAdMobBanner = true;
        public static bool ShowAdMobInterstitial = true;
        public static bool ShowAdMobRewardVideo = true;
        public static bool ShowAdMobNative = true;
        public static bool ShowAdMobAppOpen = true; //#New
        public static bool ShowAdMobRewardedInterstitial = true; //#New 

        public static string AdInterstitialKey = "ca-app-pub-5135691635931982/6657648824";
        public static string AdRewardVideoKey = "ca-app-pub-5135691635931982/7559666953";
        public static string AdAdMobNativeKey = "ca-app-pub-5135691635931982/2342769069";
        public static string AdAdMobAppOpenKey = "ca-app-pub-5135691635931982/7036343147"; //#New
        public static string AdRewardedInterstitialKey = "ca-app-pub-5135691635931982/9662506481"; //#New

        //Three times after entering the ad is displayed
        public static int ShowAdMobInterstitialCount = 3;
        public static int ShowAdMobRewardedVideoCount = 3;
        public static int ShowAdMobAppOpenCount = 2; //#New
        public static int ShowAdMobRewardedInterstitialCount = 3; //#New

        //FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowFbBannerAds = false; 
        public static bool ShowFbInterstitialAds = false; 
        public static bool ShowFbRewardVideoAds = false;  
        public static bool ShowFbNativeAds = false; 

        //YOUR_PLACEMENT_ID
        public static string AdsFbBannerKey = "250485588986218_554026418632132"; 
        public static string AdsFbInterstitialKey = "250485588986218_554026125298828";  
        public static string AdsFbRewardVideoKey = "250485588986218_554072818627492";  
        public static string AdsFbNativeKey = "250485588986218_554706301897477"; 

        //########################### 

        //Last_Messages Page >>
        ///********************************************************* 
        public static bool RunSoundControl = true;
        public static int RefreshChatActivitiesSeconds = 6000; // 6 Seconds
        public static int MessageRequestSpeed = 3000; // 3 Seconds
                  
        //Set Theme Tab
        //********************************************************* 
        public static bool SetTabDarkTheme = false; 

        //Bypass Web Errors  
        //*********************************************************
        public static bool TurnTrustFailureOnWebException = true;
        public static bool TurnSecurityProtocolType3072On = true;

        //Show custom error reporting page
        public static bool RenderPriorityFastPostLoad = true;

        //Trending 
        //*********************************************************
        public static bool ShowTrending = true; 
         
        public static bool ShowFilterBasic = true;
        public static bool ShowFilterLooks = true;
        public static bool ShowFilterBackground = true;
        public static bool ShowFilterLifestyle = true;
        public static bool ShowFilterMore = true;
          
        //*********************************************************

        //Premium system
        public static bool PremiumSystemEnabled = true;

        //Phone Validation system
        public static bool ValidationEnabled = true;
        public static bool CompressImage = false;
        public static int AvatarSize = 60;  
        public static int ImageSize = 200;    

        //Error Report Mode
        //*********************************************************
        public static bool SetApisReportMode = false; 
         
        public static bool ShowWalkTroutPage = true;

        public static bool EnableAppFree = false;

        //Payment System (ShowPaymentCardPage >> Paypal & Stripe ) (ShowLocalBankPage >> Local Bank ) 
        //*********************************************************

        public static PaymentsSystem PaymentsSystem = PaymentsSystem.All;

        public static bool ShowPaypal = true;
        public static bool ShowCreditCard = true;
        public static bool ShowBankTransfer = true;

        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// <uses-permission android:name="com.android.vending.BILLING" />
        /// </summary>
        public static bool ShowInAppBilling = false;  
        //*********************************************************

        //Settings Page >>  
        //********************************************************* 
        public static bool ShowSettingsAccount = true;  
        public static bool ShowSettingsSocialLinks = true; 
        public static bool ShowSettingsPassword = true; 
        public static bool ShowSettingsBlockedUsers = true; 
        public static bool ShowSettingsDeleteAccount = true; 
        public static bool ShowSettingsTwoFactor = true; 
        public static bool ShowSettingsManageSessions = true;  
        public static bool ShowSettingsWithdrawals = true;  
        public static bool ShowSettingsMyAffiliates = true;  
         
        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// <uses-permission android:name="android.permission.READ_CONTACTS" />
        /// <uses-permission android:name="android.permission.READ_PHONE_NUMBERS" />
        /// <uses-permission android:name="android.permission.SEND_SMS" />
        /// </summary>
        public static bool InvitationSystem = false;

        /// <summary>
        /// On main full filter view screen, reset filter option will available only on the first page by default
        /// If you want to show the reset filter option for all the pages then set "ShowResetFilterForAllPages" as true
        /// </summary>
        public static bool ShowResetFilterForAllPages = false;

        /// <summary>
        /// If want to have limit on messages then set this variable as 'true'
        /// If you set the limit on messages then non pro user will able to send only 3 messages
        /// </summary>
        public static bool ShouldHaveLimitOnMessages = true;
        //********************************************************* 
         
        public static bool ShowSettingsRateApp = true; 
        public static int ShowRateAppCount = 5; 

        public static bool ShowSettingsUpdateManagerApp = false; 
         
        public static bool OpenVideoFromApp = true; 
        public static bool OpenImageFromApp = true; 
    }
} 