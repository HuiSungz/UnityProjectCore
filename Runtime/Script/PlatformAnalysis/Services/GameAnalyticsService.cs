
using System;
using System.Collections.Generic;
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
        public bool IsInitialized => GameAnalyticsSDK.GameAnalytics.Initialized;
        
        public void LogEvent(EventData data)
        {
            string eventName = data.EventName.ToLower();
            
            GameAnalyticsSDK.GameAnalytics.NewDesignEvent(!string.IsNullOrEmpty(data.ParamValue) 
                ? $"{eventName}:{data.ParamValue}" 
                : eventName);
        }
        
        public void LogIAPEvent(IAPEventData data)
        {
            GameAnalyticsSDK.GameAnalytics.NewBusinessEvent("USD", 1, 
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
            
            GameAnalyticsSDK.GameAnalytics.NewAdEvent(
                GameAnalyticsSDK.GAAdAction.Show,
                gaAdType,
                data.Platform,
                data.NetworkName,
                gameAnalyticsParameters);
        }
        
        private GameAnalyticsSDK.GAAdType ConvertToGaAdType(AdvertisementType adType)
        {
            return adType switch
            {
                AdvertisementType.Banner => GameAnalyticsSDK.GAAdType.Banner,
                AdvertisementType.Interstitial => GameAnalyticsSDK.GAAdType.Interstitial,
                AdvertisementType.Rewarded => GameAnalyticsSDK.GAAdType.RewardedVideo,
                AdvertisementType.AppOpen => GameAnalyticsSDK.GAAdType.AppOpen,
                _ => throw new ArgumentOutOfRangeException(nameof(adType), adType, null)
            };
        }
#else
        public bool IsInitialized => false;
        
        public void LogEvent(EventData data) { }
        public void LogIAPEvent(IAPEventData data) { }
        public void LogAdRevenue(AdRevenueData data) { }
#endif
    }
}