
using Cysharp.Threading.Tasks;
using ProjectCore.PlatformService;
using ProjectCore.Utilities;
using UnityEngine;

namespace ProjectCore.Monetize
{
#if SDK_INSTALLED_APPLOVINMAX
    public class AppLovinProvider : AdProvider
    {
        #region Fields & Constructor

        private AppLovinInterstitial _interstitial;
        private AppLovinBanner _banner;
        private AppLovinReward _reward; 
        
        private bool _isInterstitialEventRegistered;
        private bool _isRewardEventRegistered;
        
        public AppLovinProvider(AdProviderType providerType) : base(providerType) { }

        #endregion

        #region Initialize

        protected override async UniTask<bool> InitializeProviderAsync()
        {
            var startTime = Time.time;
            var timeOut = AdsSetting.InitializeTimeOut;

            while (!PlatformMax.Initialized)
            {
                if (Time.time - startTime > timeOut)
                {
                    Verbose.W("[ADS] AppLovin Initialize TimeOut");
                    return false;
                }

                await UniTask.NextFrame();
            }
            
            _interstitial = new AppLovinInterstitial(MonetizationSetting);
            _reward = new AppLovinReward(MonetizationSetting);
            _banner = new AppLovinBanner();
            
            RequestInterstitial();
            RequestRewardedVideo();
            RequestBanner();
            
            return true;
        }

        #endregion
        
        public override void ShowAppOpen() { }

        #region Banner

        private void RequestBanner()
        {
            _banner?.Load(GetBannerUnitId());
        }

        public override void ShowBanner()
        {
            _banner?.Show(GetBannerUnitId());
        }

        public override void HideBanner()
        {
            _banner?.Hide(GetBannerUnitId());
        }

        public override void DestroyBanner()
        {
            _banner?.UnregisterEvents();
            _banner?.Destroy(GetBannerUnitId());
            _banner = null;
        }

        #endregion

        #region Interstitial

        public override void RequestInterstitial()
        {
            if (!_isInterstitialEventRegistered)
            {
                _interstitial?.RegisterEvents();
                _isInterstitialEventRegistered = true;
            }
            
            _interstitial?.Load(GetInterstitialUnitId());
            ADSCallback.RaiseAdLoaded(ProviderType, AdvertisementType.Interstitial);
        }

        public override void ShowInterstitial(AdvertisementCallback callback)
        {
            ADS.AppOpenAvailable = false;
            _interstitial?.Show(GetInterstitialUnitId(), OnSuccessInterstitial, OnFailureInterstitial);
        }

        private void OnSuccessInterstitial()
        {
            ADS.CallEventInMainThread(() =>
            {
                if (_isInterstitialEventRegistered)
                {
                    _interstitial?.UnregisterEvents();
                    _isInterstitialEventRegistered = false;
                }
                
                ADS.AppOpenAvailable = true;
                ADSCallback.RaiseAdDisplayed(ProviderType, AdvertisementType.Interstitial);
                ADS.ExecuteInterstitialCallback(true);
                RequestInterstitial();
            });
        }
        
        private void OnFailureInterstitial()
        {
            if (_isInterstitialEventRegistered)
            {
                _interstitial?.UnregisterEvents();
                _isInterstitialEventRegistered = false;
            }
            
            ADS.AppOpenAvailable = true;
            ADS.ExecuteInterstitialCallback(false);
            RequestInterstitial();
        }

        public override bool IsInterstitialLoaded()
        {
            return _interstitial != null && _interstitial.IsLoad(GetInterstitialUnitId());
        }

        #endregion

        #region Rewarded Video

        public override void RequestRewardedVideo()
        {
            if (!_isRewardEventRegistered)
            {
                _reward?.RegisterEvents();
                _isRewardEventRegistered = true;
            }
            
            _reward?.Load(GetRewardedVideoUnitId());
        }

        public override void ShowRewardedVideo(AdvertisementCallback callback)
        {
            ADS.AppOpenAvailable = false;
            _reward?.Show(GetRewardedVideoUnitId(), OnSuccessRewardedVideo, OnFailureRewardedVideo);
        }
        
        private void OnSuccessRewardedVideo()
        {
            ADS.CallEventInMainThread(() =>
            {
                if (_isRewardEventRegistered)
                {
                    _reward?.UnregisterEvents();
                    _isRewardEventRegistered = false;
                }
                
                ADS.AppOpenAvailable = true;
                ADSCallback.RaiseAdDisplayed(ProviderType, AdvertisementType.Rewarded);
                ADS.ExecuteRewardVideoCallback(true);
                RequestRewardedVideo();
            });
        }
        
        private void OnFailureRewardedVideo()
        {
            if (_isRewardEventRegistered)
            {
                _reward?.UnregisterEvents();
                _isRewardEventRegistered = false;
            }
            
            ADS.AppOpenAvailable = true;
            ADS.ExecuteRewardVideoCallback(false);
            RequestRewardedVideo();
        }

        public override bool IsRewardedVideoLoaded()
        {
            return _reward != null && _reward.IsLoad(GetRewardedVideoUnitId());
        }

        #endregion

        #region Utils

        private string GetInterstitialUnitId() => AdsSetting.AppLovinDefine.InterstitialID;
        private string GetRewardedVideoUnitId() => AdsSetting.AppLovinDefine.RewardID;
        private string GetBannerUnitId() => AdsSetting.AppLovinDefine.BannerID;

        #endregion
    }
#endif
}