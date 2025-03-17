
#if SDK_INSTALLED_ADMOB
using System;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using ProjectCore.PlatformService;
using ProjectCore.Utilities;

namespace ProjectCore.Monetize
{
    public class AdMobProvider : AdProvider
    {
        #region Fields

        private BannerView _bannerView;
        private InterstitialAd _interstitial;
        private RewardedAd _rewarded;
        private AppOpenAd _appOpenAd;

        private DateTime _appOpenAdExpireTime;

        #endregion
        
        public AdMobProvider(AdProviderType providerType) : base(providerType) { }

        protected override async UniTask<bool> InitializeProviderAsync()
        {
            MobileAds.SetiOSAppPauseOnBackground(true);
            ADS.CallEventInMainThread(() =>
            {
                LoadAppOpenAd();
                AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
            });

            await UniTask.Yield();
            return true;
        }

        private AdRequest GetNewAdRequest() => new();

        #region Banner

        private void RequestBanner()
        {
            
        }
        
        public override void ShowBanner()
        {
            
        }

        public override void HideBanner()
        {
            
        }

        public override void DestroyBanner()
        {
            
        }

        #endregion

        #region Interstitial

        public override void RequestInterstitial()
        {
            
        }

        public override void ShowInterstitial(AdvertisementCallback callback)
        {
            
        }

        public override bool IsInterstitialLoaded()
        {
            return false;
        }

        #endregion

        #region Rewarded

        public override void RequestRewardedVideo()
        {
            
        }

        public override void ShowRewardedVideo(AdvertisementCallback callback)
        {
            
        }

        public override bool IsRewardedVideoLoaded()
        {
            return false;
        }

        #endregion

        #region App Open

        public override void ShowAppOpen()
        {
            if (IsAppOpenAdAvailable())
            {
                _appOpenAd.Show();
            }
        }

        private bool IsAppOpenAdAvailable()
        {
            return _appOpenAd != null
                   && _appOpenAd.CanShowAd()
                   && DateTime.Now < _appOpenAdExpireTime;
        }

        private void OnAppStateChanged(AppState state)
        {
            if (state == AppState.Foreground)
            {
                ADS.ShowAppOpen();
            }
        }

        private void LoadAppOpenAd()
        {
            _appOpenAd?.Destroy();
            _appOpenAd = null;
            
            AppOpenAd.Load(GetAppOpenId(), GetNewAdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Verbose.E("[ADS] AdMob App Open Ad failed to load: " + error);
                    return;
                }

                _appOpenAd = ad;
                _appOpenAdExpireTime = DateTime.Now + TimeSpan.FromHours(2);

                RegisterAppOpenAdEvents(ad);
            });
        }

        private void RegisterAppOpenAdEvents(AppOpenAd ad)
        {
            ad.OnAdPaid += HandleAppOpenAdPaid;
            ad.OnAdFullScreenContentClosed += HandleAppOpenAdClosed;
            ad.OnAdFullScreenContentFailed += HandleAppOpenAdFailed;
        }

        private void HandleAppOpenAdPaid(AdValue adValue)
        {
            ADSCallback.RaiseAdRevenueAdMob(adValue);
        }

        private void HandleAppOpenAdFailed(AdError obj)
        {
            Verbose.E("[ADS] AdMob App Open Ad failed: " + obj);
            
            LoadAppOpenAd();
        }

        private void HandleAppOpenAdClosed()
        {
            Verbose.D("[ADS] AdMob App Open Ad closed");
            
            ADSCallback.RaiseAppOpenClosed(ProviderType);
            LoadAppOpenAd();
        }

        #endregion

        #region Utils

        private string GetAppOpenId()
        {
#if UNITY_ANDROID
            return AdsSetting.AdMobDefine.AndroidAppOpenID;
#elif UNITY_IOS
            return AdsSetting.AdMobDefine.IosAppOpenID;
#endif
        }
        
        #endregion
    }
}
#endif