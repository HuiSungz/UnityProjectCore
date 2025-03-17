
using System;
using System.Collections.Generic;
#if !SDK_INSTALLED_GAMEANALYTICS
using System.Diagnostics;
#endif
using GameAnalyticsSDK;
using ProjectCore.Monetize;

namespace ProjectCore.PlatformAnalysis
{
    /// <summary>
    /// GameAnalytics를 사용한 통합 분석 서비스
    /// </summary>
    internal class GameAnalyticsService : IAnalyticsService
    {
        public AnalysisType Type => AnalysisType.GameAnalytics;
        
#if SDK_INSTALLED_GAMEANALYTICS
        public bool IsInitialized => GameAnalytics.Initialized;
        
        public void LogEvent(EventData data)
        {
            string eventName = data.EventName.ToLower();
            
            GameAnalytics.NewDesignEvent(!string.IsNullOrEmpty(data.ParamValue) 
                ? $"{eventName}:{data.ParamValue}" 
                : eventName);
        }
        
        public void LogIAPEvent(IAPEventData data)
        {
            GameAnalytics.NewBusinessEvent("USD", 1, 
                data.TransactionId, 
                data.ProductId, 
                data.StoreKey, 
                data.Attributes);
        }
        
        public void LogAdRevenue(AdRevenueData data)
        {
            var gameAnalyticsParameters = new Dictionary<string, object>
            {
                { "currency", data.CurrencyCode },
                { "revenue", data.Revenue }
            };
            
            if (!string.IsNullOrEmpty(data.Precision))
            {
                gameAnalyticsParameters.Add("estimated", data.Precision);
            }
            
            var gaAdType = ConvertToGaAdType(data.AdType);
            
            GameAnalytics.NewAdEvent(
                GAAdAction.Show,
                gaAdType,
                data.Platform,
                data.PlacementName ?? data.AdType.ToString(),
                gameAnalyticsParameters);
        }
        
        private GAAdType ConvertToGaAdType(AdvertisementType adType)
        {
            return adType switch
            {
                AdvertisementType.Banner => GAAdType.Banner,
                AdvertisementType.Interstitial => GAAdType.Interstitial,
                AdvertisementType.Rewarded => GAAdType.RewardedVideo,
                AdvertisementType.AppOpen => GAAdType.AppOpen,
                _ => throw new ArgumentOutOfRangeException(nameof(adType), adType, null)
            };
        }
#else
        public bool IsInitialized => false;
        
        [Conditional("SDK_INSTALLED_GAMEANALYTICS")]
        public void LogEvent(EventData data) { }
        
        [Conditional("SDK_INSTALLED_GAMEANALYTICS")]
        public void LogIAPEvent(IAPEventData data) { }
        
        [Conditional("SDK_INSTALLED_GAMEANALYTICS")]
        public void LogAdRevenue(AdRevenueData data) { }
#endif
    }
}