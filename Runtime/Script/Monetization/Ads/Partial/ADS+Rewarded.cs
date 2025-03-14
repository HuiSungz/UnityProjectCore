
using System;
using ProjectCore.Utilities;
using UnityEngine;

namespace ProjectCore.Monetize
{
    public static partial class ADS
    {
        #region Fields

        private static AdProvider.AdvertisementCallback _rewardedCallback;
        private static Coroutine _rewardedCoroutine;

        private static bool _waitingForRewardVideoCallback;

        #endregion

        #region Request

        public static void RequestRewardVideo()
        {
            if(!Monetization.IsActivate || !_isConfigure)
            {
                Verbose.W("[ADS] Monetization is not activated or not configured.");
                return;
            }
            
            var adProviderType = Setting.RewardedType;
            if(ValidateRequestRewardVideo(adProviderType))
            {
                return;
            }
            
            _advertisementProviders[adProviderType].RequestRewardedVideo();
        }

        #endregion

        #region Show

        public static void ShowRewardVideo(AdProvider.AdvertisementCallback callback)
        {
            try
            {
                if (!Monetization.IsActivate || !_isConfigure)
                {
                    Verbose.W("[ADS] Monetization is not activated or not configured.");
                    ExecuteRewardVideoCallback(false);
                    return;
                }

                var adProviderType = Setting.RewardedType;
                _rewardedCallback = callback;
                _waitingForRewardVideoCallback = true;
                
                if (ValidateShowRewardVideo(adProviderType))
                {
                    Verbose.W("[ADS] Can't show rewarded.");
                    ExecuteRewardVideoCallback(false);
                    return;
                }

                if (Setting.DelayInvokeDuration > 0f)
                {
                    ADSCallback.RaiseShowLoading();
                    Executor.CancelDelayExecute(_rewardedCoroutine);
                    
                    _rewardedCoroutine = Executor.CallDelayExecute(Setting.DelayInvokeDuration, () =>
                    {
                        _advertisementProviders[adProviderType].ShowRewardedVideo(callback);
                        ADSCallback.RaiseHideLoading();
                    });
                }
                else
                {
                    _advertisementProviders[adProviderType].ShowRewardedVideo(callback);
                }
            }
            catch (Exception exception)
            {
                Verbose.Ex("[ADS] ShowRewardVideo failed. : ", exception);
            }
        }

        #endregion

        #region Utils - Public

        public static bool IsRewardVideoLoaded()
        {
            if(!Monetization.IsActivate || !_isConfigure)
            {
                Verbose.W("[ADS] Monetization is not activated or not configured.");
                return false;
            }
            
            var adProviderType = Setting.RewardedType;
            return IsProviderActive(adProviderType) && _advertisementProviders[adProviderType].IsRewardedVideoLoaded();
        }

        #endregion

        #region Utils - Private & Internal

        internal static void ExecuteRewardVideoCallback(bool isSuccess)
        {
            if (_rewardedCallback == null || !_waitingForRewardVideoCallback)
            {
                return;
            }
            
            CallEventInMainThread(() => _rewardedCallback.Invoke(isSuccess));
            _waitingForRewardVideoCallback = false;
        }

        private static bool ValidateRequestRewardVideo(AdProviderType providerType)
        {
            return !IsProviderActive(providerType) 
                   || !_advertisementProviders[providerType].IsInitialized 
                   || _advertisementProviders[providerType].IsRewardedVideoLoaded();
        }
        
        private static bool ValidateShowRewardVideo(AdProviderType providerType)
        {
            return !IsProviderActive(providerType) 
                   || !_advertisementProviders[providerType].IsInitialized 
                   || !_advertisementProviders[providerType].IsRewardedVideoLoaded();
        }

        #endregion
    }
}