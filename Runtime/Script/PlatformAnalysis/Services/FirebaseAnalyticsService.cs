
#if !SDK_INSTALLED_FIREBASE
using System.Diagnostics;
#endif
using ProjectCore.PlatformService;

namespace ProjectCore.PlatformAnalysis
{
    /// <summary>
    /// Firebase를 사용한 통합 분석 서비스
    /// </summary>
    internal class FirebaseAnalyticsService : IAnalyticsService
    {
        public AnalysisType Type => AnalysisType.Firebase;
        
#if SDK_INSTALLED_FIREBASE
        public bool IsInitialized => PlatformFirebase.Initialized;
        
        public void LogEvent(EventData data)
        {
            string eventName = data.EventName.ToLower();
            
            if (!string.IsNullOrEmpty(data.ParamValue))
            {
                var parameters = new Firebase.Analytics.Parameter(eventName, data.ParamValue);
                Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, parameters);
            }
            else
            {
                Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
            }
        }
        
        public void LogIAPEvent(IAPEventData data)
        {
            var inAppPurchaseParams = new[] 
            {
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterAdPlatform, data.StoreKey),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterItemName, data.ProductId),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.EventPurchase, data.Revenue)
            };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("in_app_purchase", inAppPurchaseParams);
        }
        
        public void LogAdRevenue(AdRevenueData data)
        {
            Firebase.Analytics.Parameter[] parameters;
            
            if (!string.IsNullOrEmpty(data.Precision))
            {
                parameters = new[]
                {
                    new Firebase.Analytics.Parameter("ad_impression", data.AdType.ToString()),
                    new Firebase.Analytics.Parameter("ad_platform", data.Platform),
                    new Firebase.Analytics.Parameter("value", data.Revenue),
                    new Firebase.Analytics.Parameter("estimated", data.Precision),
                    new Firebase.Analytics.Parameter("currency", data.CurrencyCode)
                };
            }
            else
            {
                parameters = new[]
                {
                    new Firebase.Analytics.Parameter("ad_type", data.AdType.ToString()),
                    new Firebase.Analytics.Parameter("ad_platform", data.Platform),
                    new Firebase.Analytics.Parameter("ad_source", data.NetworkName),
                    new Firebase.Analytics.Parameter("ad_unit_name", data.AdUnitId),
                    new Firebase.Analytics.Parameter("ad_format", data.AdFormat),
                    new Firebase.Analytics.Parameter("value", data.Revenue),
                    new Firebase.Analytics.Parameter("currency", data.CurrencyCode)
                };
            }
            
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", parameters);
        }
#else
        public bool IsInitialized => false;
        
        [Conditional("SDK_INSTALLED_FIREBASE")]
        public void LogEvent(EventData data) { }
        
        [Conditional("SDK_INSTALLED_FIREBASE")]
        public void LogIAPEvent(IAPEventData data) { }
        
        [Conditional("SDK_INSTALLED_FIREBASE")]
        public void LogAdRevenue(AdRevenueData data) { }
#endif
    }
}