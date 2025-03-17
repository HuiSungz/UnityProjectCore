
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.PlatformAnalysis
{
    internal class AnalysisPresenter
    {
        #region Fields & Constructor

        private readonly List<IAnalyticsService> _services;
        private readonly Dictionary<AnalysisType, IAnalyticsService> _serviceMap;

        internal AnalysisPresenter(List<IAnalyticsService> services)
        {
            _services = services;
            _serviceMap = new Dictionary<AnalysisType, IAnalyticsService>();
            
            foreach (var service in services)
            {
                _serviceMap.Add(service.Type, service);
            }
        }

        #endregion
        
        /// <summary>
        /// 지정된 분석 타입에 이벤트를 로깅합니다.
        /// </summary>
        /// <param name="data">이벤트 데이터</param>
        /// <param name="analysisType">로깅할 분석 타입</param>
        public void LogEvent(EventData data, AnalysisType analysisType)
        {
            if (analysisType == AnalysisType.Unknown)
            {
                return;
            }
            
            var targetTypes = GetAnalysisTypeFlags(analysisType);
            
            foreach (var type in targetTypes)
            {
                if (!_serviceMap.TryGetValue(type, out var service))
                {
                    continue;
                }
                
                if (!service.IsInitialized)
                {
                    Debug.LogWarning($"[Analysis] {type} 서비스가 초기화되지 않았습니다. 이벤트: {data.EventName}");
                    continue;
                }
                
                try
                {
                    service.LogEvent(data);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[Analysis] 이벤트 로깅 실패: {data.EventName}, 서비스: {type}, 오류: {exception.Message}");
                }
            }
        }
        
        /// <summary>
        /// IAP 이벤트를 초기화된 모든 서비스에 로깅합니다.
        /// </summary>
        /// <param name="data">IAP 이벤트 데이터</param>
        public void LogIAPEvent(IAPEventData data)
        {
            foreach (var service in _services)
            {
                if (!service.IsInitialized)
                {
                    Debug.LogWarning($"[Analysis] {service.Type} 서비스가 초기화되지 않았습니다. IAP 이벤트: {data.ProductId}");
                    continue;
                }
                
                try
                {
                    service.LogIAPEvent(data);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[Analysis] IAP 이벤트 로깅 실패: {data.ProductId}, 서비스: {service.Type}, 오류: {exception.Message}");
                }
            }
        }
        
        /// <summary>
        /// 광고 수익 이벤트를 초기화된 모든 서비스에 로깅합니다.
        /// </summary>
        /// <param name="data">광고 수익 데이터</param>
        public void LogAdRevenue(AdRevenueData data)
        {
            foreach (var service in _services)
            {
                if (!service.IsInitialized)
                {
                    Debug.LogWarning($"[Analysis] {service.Type} 서비스가 초기화되지 않았습니다. 광고 수익 이벤트: {data.Platform}");
                    continue;
                }
                
                try
                {
                    service.LogAdRevenue(data);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[Analysis] 광고 수익 로깅 실패: {data.Platform}, 서비스: {service.Type}, 오류: {exception.Message}");
                }
            }
        }
        
        /// <summary>
        /// AnalysisType 플래그 열거형에서 설정된 플래그 목록을 추출합니다.
        /// </summary>
        private List<AnalysisType> GetAnalysisTypeFlags(AnalysisType type)
        {
            var result = new List<AnalysisType>();
            
            if (type.HasFlag(AnalysisType.Singular)) result.Add(AnalysisType.Singular);
            if (type.HasFlag(AnalysisType.GameAnalytics)) result.Add(AnalysisType.GameAnalytics);
            if (type.HasFlag(AnalysisType.Firebase)) result.Add(AnalysisType.Firebase);
            
            return result;
        }
    }
}