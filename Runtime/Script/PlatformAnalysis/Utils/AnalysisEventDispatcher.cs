
using ProjectCore.Monetize;
using UnityEngine;
using UnityEngine.Purchasing;

namespace ProjectCore.PlatformAnalysis
{
    internal class AnalysisEventDispatcher : MonoBehaviour
    {
        #region Fields

        private AnalysisSettings _settings;

        #endregion

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        public void Initialize(AnalysisSettings settings)
        {
            _settings = settings;
            
            if (settings.AutoSendADSRevenue)
            {
#if SDK_INSTALLED_ADMOB
                ADSCallback.OnAdRevenueAdMob += RevenueCallback;
#endif
#if SDK_INSTALLED_APPLOVINMAX
                ADSCallback.OnAdRevenueAppLovin += RevenueCallback;
#endif
            }

            if (settings.AutoSendIAPRevenue)
            {
                IAPCallback.OnPurchaseCompletedWithArgs += RevenueIAPCallback;
            }
        }

        private void OnDisable()
        {
            if (_settings.AutoSendADSRevenue)
            {
#if SDK_INSTALLED_ADMOB
                ADSCallback.OnAdRevenueAdMob -= RevenueCallback;
#endif
#if SDK_INSTALLED_APPLOVINMAX
                ADSCallback.OnAdRevenueAppLovin -= RevenueCallback;
#endif
            }

            if (_settings.AutoSendIAPRevenue)
            {
                IAPCallback.OnPurchaseCompletedWithArgs -= RevenueIAPCallback;
            }
        }

#if SDK_INSTALLED_ADMOB
        private void RevenueCallback(GoogleMobileAds.Api.AdValue adValue)
        {
            Analysis.LogAdMobRevenue(adValue);
        }
#endif

#if SDK_INSTALLED_APPLOVINMAX
        private void RevenueCallback(MaxSdkBase.AdInfo adInfo, AdvertisementType advertisementType)
        {
            Analysis.LogAppLovinRevenue(adInfo, advertisementType);
        }
#endif
        
        private void RevenueIAPCallback(PurchaseEventArgs purchaseEventArgs)
        {
            var storeKey = Application.platform == RuntimePlatform.Android 
                ? "GooglePlay" 
                : "AppleAppStore";
            Analysis.LogIAPEvent(storeKey, purchaseEventArgs);
        }
    }
}