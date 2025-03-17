
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
        
        public void Initialize(AnalysisSettings settings)
        {
            _settings = settings;
            
            if (settings.AutoSendADSRevenue)
            {
                ADSCallback.OnAdRevenueAdMob += RevenueCallback;
                ADSCallback.OnAdRevenueAppLovin += RevenueCallback;
            }

            if (settings.AutoSendIAPRevenue)
            {
                
            }
        }

        private void OnDisable()
        {
            if (_settings.AutoSendADSRevenue)
            {
                ADSCallback.OnAdRevenueAdMob -= RevenueCallback;
                ADSCallback.OnAdRevenueAppLovin -= RevenueCallback;
            }

            if (_settings.AutoSendIAPRevenue)
            {
                
            }
        }

        private void RevenueCallback(GoogleMobileAds.Api.AdValue adValue)
        {
            Analysis.LogAdMobRevenue(adValue);
        }

        private void RevenueCallback(MaxSdkBase.AdInfo adInfo, AdvertisementType advertisementType)
        {
            Analysis.LogAppLovinRevenue(adInfo, advertisementType);
        }
        
        private void RevenueIAPCallback(PurchaseEventArgs purchaseEventArgs)
        {
            var storeKey = Application.platform == RuntimePlatform.Android 
                ? "GooglePlay" 
                : "AppleAppStore";
            Analysis.LogIAPEvent(storeKey, purchaseEventArgs);
        }
    }
}