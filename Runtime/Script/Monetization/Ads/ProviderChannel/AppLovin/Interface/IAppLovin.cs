
using System;

namespace ProjectCore.Monetize
{
    internal interface IAppLovin
    {
        /// <summary>
        /// 광고 이벤트 등록
        /// </summary>
        void RegisterEvents();
        
        /// <summary>
        /// 광고 이벤트 해제
        /// </summary>
        void UnregisterEvents();
        
        /// <summary>
        /// 광고가 로드되었는지 확인
        /// </summary>
        /// <param name="unitId">광고 유닛 ID</param>
        /// <returns>로드 여부</returns>
        bool IsLoad(string unitId);
        
        /// <summary>
        /// 광고 로드 요청
        /// </summary>
        /// <param name="unitId">광고 유닛 ID</param>
        void Load(string unitId);
        
        /// <summary>
        /// 광고 표시
        /// </summary>
        /// <param name="unitId">광고 유닛 ID</param>
        /// <param name="successAction">성공 콜백</param>
        /// <param name="failureAction">실패 콜백</param>
        void Show(string unitId, Action successAction, Action failureAction);
    }
}