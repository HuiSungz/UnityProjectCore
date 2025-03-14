
using System;
using ProjectCore.Utilities;

namespace ProjectCore.Monetize
{
    /// <summary>
    /// 광고 이벤트 콜백을 처리하기 위한 정적 클래스
    /// </summary>
    public static class ADSCallback
    {
        #region Event Definitions

        // 광고 제공자 초기화 이벤트
        public static event Action<AdProviderType> OnAdProviderInitialized;
        // 광고 로드 이벤트
        public static event Action<AdProviderType, AdvertisementType> OnAdLoaded;
        // 광고 표시 이벤트
        public static event Action<AdProviderType, AdvertisementType> OnAdDisplayed;
        // 광고 종료 이벤트
        public static event Action<AdProviderType, AdvertisementType> OnAdClosed;
        // 광고 앱오픈 종료 이벤트
        public static event Action<AdProviderType> OnAppOpenClosed;
        
        // 로딩 관련 이벤트
        public static event Action OnDelayInvokeStarted;
        public static event Action OnDelayInvokeFinished;

        #endregion

        #region Internal Event Raisers
        
        internal static void RaiseAdProviderInitialized(AdProviderType providerType)
        {
            try
            {
                OnAdProviderInitialized?.Invoke(providerType);
            }
            catch (Exception ex)
            {
                Verbose.E($"[ADSCallback] Error in OnAdProviderInitialized event: {ex}");
            }
        }
        
        internal static void RaiseAdLoaded(AdProviderType providerType, AdvertisementType adType)
        {
            try
            {
                OnAdLoaded?.Invoke(providerType, adType);
            }
            catch (Exception ex)
            {
                Verbose.E($"[ADSCallback] Error in OnAdLoaded event: {ex}");
            }
        }
        
        internal static void RaiseAdDisplayed(AdProviderType providerType, AdvertisementType adType)
        {
            try
            {
                OnAdDisplayed?.Invoke(providerType, adType);
                if (adType is AdvertisementType.Interstitial)
                {
                    ADS.ResetInterstitialDelayTime();
                }
            }
            catch (Exception ex)
            {
                Verbose.E($"[ADSCallback] Error in OnAdDisplayed event: {ex}");
            }
        }

        internal static void RaiseAdClosed(AdProviderType providerType, AdvertisementType adType)
        {
            try
            {
                OnAdClosed?.Invoke(providerType, adType);
                if (adType is AdvertisementType.Interstitial)
                {
                    ADS.ResetInterstitialDelayTime();
                }
            }
            catch (Exception ex)
            {
                Verbose.E($"[ADSCallback] Error in OnAdClosed event: {ex}");
            }
        }
        
        internal static void RaiseAppOpenClosed(AdProviderType providerType)
        {
            try
            {
                OnAppOpenClosed?.Invoke(providerType);
                ADS.ResetAppOpenDelayTime();
            }
            catch (Exception ex)
            {
                Verbose.E($"[ADSCallback] Error in OnAppOpenClosed event: {ex}");
            }
        }

        #endregion

        #region Loading UI Event Handling

        /// <summary>
        /// 로딩 시작 이벤트 발생
        /// </summary>
        internal static void RaiseShowLoading()
        {
            try
            {
                OnDelayInvokeStarted?.Invoke();
            }
            catch (Exception ex)
            {
                Verbose.E($"[ADSCallback] Error in OnLoadingStarted event: {ex}");
            }
        }
        
        /// <summary>
        /// 로딩 종료 이벤트 발생
        /// </summary>
        internal static void RaiseHideLoading()
        {
            try
            {
                OnDelayInvokeFinished?.Invoke();
            }
            catch (Exception ex)
            {
                Verbose.E($"[ADSCallback] Error in OnLoadingFinished event: {ex}");
            }
        }

        #endregion
    }
}