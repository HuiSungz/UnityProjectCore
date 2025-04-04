
using UnityEngine;

namespace ProjectCore.Monetize
{
#if SDK_INSTALLED_APPLOVINMAX
    internal sealed class AppLovinBanner
    {
        #region Fields

        private bool _activateBanner;

        #endregion
        
        public void RegisterEvents()
        {
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        }

        public void UnregisterEvents()
        {
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnAdRevenuePaidEvent;
        }
        
        public void Load(string unitId)
        {
            if (_activateBanner)
            {
                return;
            }
            
            MaxSdk.CreateBanner(unitId, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerBackgroundColor(unitId, Color.clear);
            RegisterEvents();
        }

        public void Show(string unitId)
        {
            if (_activateBanner)
            {
                return;
            }
            
            _activateBanner = true;
            MaxSdk.ShowBanner(unitId);
        }

        public void Hide(string unitId)
        {
            if (!_activateBanner)
            {
                return;
            }

            _activateBanner = false;
            MaxSdk.HideBanner(unitId);
        }
        
        public void Destroy(string unitId) => MaxSdk.DestroyBanner(unitId);
        
        private void OnAdRevenuePaidEvent(string unitId, MaxSdkBase.AdInfo adInfo)
        {
            ADSCallback.RaiseAdRevenueAppLovin(adInfo, AdvertisementType.Banner);
        }
    }
#endif
}