
using System.Collections.Generic;
using ProjectCore.Monetize;
using UnityEngine.Purchasing;

namespace ProjectCore.PlatformAnalysis
{
    /// <summary>
    /// 분석 서비스와 데이터를 생성하는 팩토리 클래스
    /// </summary>
    internal static class AnalysisServiceFactory
    {
        /// <summary>
        /// 모든 분석 서비스를 생성합니다.
        /// </summary>
        public static List<IAnalyticsService> CreateServices()
        {
            return new List<IAnalyticsService>
            {
                new FirebaseAnalyticsService(),
                new SingularAnalyticsService(),
                new GameAnalyticsService()
            };
        }
        
        /// <summary>
        /// 일반 이벤트 데이터를 생성합니다.
        /// </summary>
        /// <param name="eventName">이벤트 이름</param>
        /// <param name="paramValue">파라미터 값 (선택사항)</param>
        /// <returns>이벤트 데이터</returns>
        public static EventData CreateEventData(string eventName, string paramValue = null)
        {
            return new EventData.Builder(eventName)
                .SetParamValue(paramValue)
                .Build();
        }
        
        /// <summary>
        /// IAP 이벤트 데이터를 생성합니다.
        /// </summary>
        /// <param name="storeKey">스토어 키</param>
        /// <param name="purchaseEventArgs">구매 이벤트 인자</param>
        /// <returns>IAP 이벤트 데이터</returns>
        public static IAPEventData CreateIAPEventData(string storeKey, PurchaseEventArgs purchaseEventArgs)
        {
            var product = purchaseEventArgs.purchasedProduct;
            var revenue = product.metadata.localizedPriceString;
            
            var attributes = new Dictionary<string, object>
            {
                { "Revenue", revenue }
            };
            
            return new IAPEventData.Builder(storeKey, product)
                .SetRevenue(revenue)
                .SetAttributes(attributes)
                .Build();
        }
        
#if SDK_INSTALLED_ADMOB
        /// <summary>
        /// AdMob 광고 데이터로부터 AdRevenueData를 생성합니다.
        /// </summary>
        /// <param name="adValue">AdMob 광고 값</param>
        /// <param name="adType">광고 유형</param>
        /// <returns>광고 수익 데이터</returns>
        public static AdRevenueData CreateFromAdMob(GoogleMobileAds.Api.AdValue adValue, AdvertisementType adType = AdvertisementType.AppOpen)
        {
            return new AdRevenueData.Builder("Admob", adValue.Value / 1000000d)
                .SetCurrencyCode(adValue.CurrencyCode)
                .SetPrecision(adValue.Precision.ToString())
                .SetAdType(adType)
                .Build();
        }
#endif
        
#if SDK_INSTALLED_APPLOVINMAX
        /// <summary>
        /// AppLovin 광고 데이터로부터 AdRevenueData를 생성합니다.
        /// </summary>
        /// <param name="adInfo">AppLovin 광고 정보</param>
        /// <param name="adType">광고 유형</param>
        /// <returns>광고 수익 데이터</returns>
        public static AdRevenueData CreateFromAppLovin(MaxSdkBase.AdInfo adInfo, AdvertisementType adType)
        {
            return new AdRevenueData.Builder("AppLovin", adInfo.Revenue)
                .SetCurrencyCode("USD")
                .SetNetworkName(adInfo.NetworkName)
                .SetAdUnitId(adInfo.AdUnitIdentifier)
                .SetPlacementName(adInfo.Placement)
                .SetAdFormat(adInfo.AdFormat)
                .SetAdType(adType)
                .Build();
        }
#endif
    }
}