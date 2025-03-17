
namespace ProjectCore.PlatformAnalysis
{
    /// <summary>
    /// 통합 분석 서비스 인터페이스
    /// </summary>
    internal interface IAnalyticsService
    {
        /// <summary>
        /// 일반 이벤트를 로깅합니다.
        /// </summary>
        /// <param name="data">이벤트 데이터</param>
        void LogEvent(EventData data);
        
        /// <summary>
        /// IAP 이벤트를 로깅합니다.
        /// </summary>
        /// <param name="data">IAP 이벤트 데이터</param>
        void LogIAPEvent(IAPEventData data);
        
        /// <summary>
        /// 광고 수익 이벤트를 로깅합니다.
        /// </summary>
        /// <param name="data">광고 수익 데이터</param>
        void LogAdRevenue(AdRevenueData data);
        
        /// <summary>
        /// 서비스가 초기화되었는지 여부
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 이 서비스에 연결된 미디어 타입
        /// </summary>
        AnalysisType Type { get; }
    }
}