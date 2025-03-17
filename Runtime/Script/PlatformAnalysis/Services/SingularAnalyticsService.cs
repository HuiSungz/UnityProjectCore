
#if !SDK_INSTALLED_SINGULAR
using System.Diagnostics;
#endif
using ProjectCore.PlatformService;

namespace ProjectCore.PlatformAnalysis
{
    /// <summary>
    /// Singular를 사용한 통합 분석 서비스
    /// </summary>
    internal class SingularAnalyticsService : IAnalyticsService
    {
        public AnalysisType Type => AnalysisType.Singular;
        
#if SDK_INSTALLED_SINGULAR
        public bool IsInitialized => PlatformSingular.Initialized;
        
        public void LogEvent(EventData data)
        {
            var formattedEvent = GetFormattedEvent(data.EventName, data.ParamValue);
            Singular.SingularSDK.Event(formattedEvent);
        }
        
        public void LogIAPEvent(IAPEventData data)
        {
            Singular.SingularSDK.InAppPurchase(data.Product, data.Attributes);
        }
        
        public void LogAdRevenue(AdRevenueData data)
        {
            var singularData = new Singular.SingularAdData(
                data.Platform, 
                data.CurrencyCode, 
                data.Revenue);
            
            if (!string.IsNullOrEmpty(data.AdUnitId))
            {
                singularData.WithAdUnitId(data.AdUnitId);
            }
            
            if (!string.IsNullOrEmpty(data.NetworkName))
            {
                singularData.WithNetworkName(data.NetworkName);
            }
            
            if (!string.IsNullOrEmpty(data.PlacementName))
            {
                singularData.WithAdPlacmentName(data.PlacementName);
            }
            
            Singular.SingularSDK.AdRevenue(singularData);
        }
        
        private string GetFormattedEvent(string eventName, string paramValue)
        {
            eventName = eventName.ToLower();
            return string.IsNullOrEmpty(paramValue) ? $"{eventName}" : $"{eventName}_{paramValue}";
        }
#else
        public bool IsInitialized => false;
        
        [Conditional("SDK_INSTALLED_SINGULAR")]
        public void LogEvent(EventData data) { }
        
        [Conditional("SDK_INSTALLED_SINGULAR")]
        public void LogIAPEvent(IAPEventData data) { }
        
        [Conditional("SDK_INSTALLED_SINGULAR")]
        public void LogAdRevenue(AdRevenueData data) { }
#endif
    }
}