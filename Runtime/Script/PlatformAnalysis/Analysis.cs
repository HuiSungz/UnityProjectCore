
using ProjectCore.Monetize;
using UnityEngine;
using UnityEngine.Purchasing;

namespace ProjectCore.PlatformAnalysis
{
    public sealed class AnalysisSettings
    {
        public bool AutoSendADSRevenue { get; private set; }
        public bool AutoSendIAPRevenue { get; private set; }
        
        public AnalysisSettings(bool autoSendADSRevenue, bool autoSendIAPRevenue)
        {
            AutoSendADSRevenue = autoSendADSRevenue;
            AutoSendIAPRevenue = autoSendIAPRevenue;
        }
    }

    /// <summary>
    /// 모든 분석 기능을 제공하는 정적 API 클래스
    /// </summary>
    public static class Analysis
    {
        private static AnalysisPresenter _presenter;

        public static void Initialize(AnalysisSettings settings)
        {
            var services = AnalysisServiceFactory.CreateServices();
            _presenter = new AnalysisPresenter(services);

            if (!settings.AutoSendADSRevenue && !settings.AutoSendIAPRevenue)
            {
                return;
            }
            
            var dispatcher = new GameObject("[ANALYSIS EVENT DISPATCHER]")
                .AddComponent<AnalysisEventDispatcher>();
            dispatcher.Initialize(settings);
        }
        
        #region 일반 이벤트 로깅
        
        /// <summary>
        /// 지정된 분석 타입에 이벤트를 로깅합니다.
        /// </summary>
        /// <param name="eventName">이벤트 이름</param>
        /// <param name="paramValue">파라미터 값 (선택사항)</param>
        /// <param name="analysisType">로깅할 분석 타입</param>
        public static void LogEvent(string eventName, string paramValue = null, AnalysisType analysisType = AnalysisType.All)
        {
            var data = AnalysisServiceFactory.CreateEventData(eventName, paramValue);
            _presenter.LogEvent(data, analysisType);
        }
        
        #endregion
        
        #region IAP 이벤트 로깅
        
        /// <summary>
        /// IAP 구매 이벤트를 로깅합니다.
        /// </summary>
        /// <param name="storeKey">스토어 키</param>
        /// <param name="purchaseEventArgs">구매 이벤트 인자</param>
        public static void LogIAPEvent(string storeKey, PurchaseEventArgs purchaseEventArgs)
        {
            var data = AnalysisServiceFactory.CreateIAPEventData(storeKey, purchaseEventArgs);
            _presenter.LogIAPEvent(data);
        }
        
        #endregion
        
        #region 광고 수익 로깅
        
#if SDK_INSTALLED_ADMOB
        /// <summary>
        /// AdMob 광고 수익을 추적합니다.
        /// </summary>
        /// <param name="adValue">AdMob 광고 값</param>
        /// <param name="adType">광고 유형 (기본값: AppOpen)</param>
        public static void LogAdMobRevenue(GoogleMobileAds.Api.AdValue adValue, AdvertisementType adType = AdvertisementType.AppOpen)
        {
            var data = AnalysisServiceFactory.CreateFromAdMob(adValue, adType);
            _presenter.LogAdRevenue(data);
        }
#endif

#if SDK_INSTALLED_APPLOVINMAX
        /// <summary>
        /// AppLovin 광고 수익을 추적합니다.
        /// </summary>
        /// <param name="adInfo">AppLovin 광고 정보</param>
        /// <param name="adType">광고 유형</param>
        public static void LogAppLovinRevenue(MaxSdkBase.AdInfo adInfo, AdvertisementType adType)
        {
            var data = AnalysisServiceFactory.CreateFromAppLovin(adInfo, adType);
            _presenter.LogAdRevenue(data);
        }
#endif
        
        #endregion
    }
}