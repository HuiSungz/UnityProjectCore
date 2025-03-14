
using System;
using System.Linq;
using ProjectCore.Utilities;
using UnityEngine;

namespace ProjectCore.Monetize
{
    public static partial class ADS
    {
        #region Fields

        private static AdProvider.AdvertisementCallback _interstitialCallback;
        private static Coroutine _interstitialCoroutine;

        private static float _lastInterstitialTime;

        #endregion

        #region Request

        public static void RequestInterstitial()
        {
            if(!Monetization.IsActivate || !_isConfigure)
            {
                Verbose.W("[ADS] Monetization is not activated or not configured.");
                return;
            }
            
            var adProviderType = Setting.InterstitialType;
            if (ValidateRequestInterstitial(adProviderType))
            {
                return;
            }
            
            _advertisementProviders[adProviderType].RequestInterstitial();
        }

        #endregion

        #region Show

        public static void ShowInterstitial(AdProvider.AdvertisementCallback callback, bool ignoreCondition = false)
        {
            try
            {
                if (!Monetization.IsActivate || !_isConfigure)
                {
                    Verbose.W("[ADS] Monetization is not activated or not configured.");
                    ExecuteInterstitialCallback(false);
                    return;
                }

                var adProviderType = Setting.InterstitialType;
                _interstitialCallback = callback;
                if (ValidateShowInterstitial(adProviderType, ignoreCondition))
                {
                    Verbose.W("[ADS] Can't show interstitial.");
                    ExecuteInterstitialCallback(false);
                    return;
                }
                
                if (Setting.DelayInvokeDuration > 0f)
                {
                    ADSCallback.RaiseShowLoading();
                    Executor.CancelDelayExecute(_interstitialCoroutine);
                    
                    _interstitialCoroutine = Executor.CallDelayExecute(_setting.DelayInvokeDuration, () =>
                    {
                        _advertisementProviders[adProviderType].ShowInterstitial(callback);
                        ADSCallback.RaiseHideLoading();
                    });
                }
                else
                {
                    _advertisementProviders[adProviderType].ShowInterstitial(callback);
                }
            }
            catch (Exception exception)
            {
                Verbose.Ex("[ADS] ShowInterstitial failed. : ", exception);
            }
        }

        #endregion

        #region Utils - Public

        public static bool IsInterstitialLoaded()
        {
            if(!Monetization.IsActivate || !_isConfigure)
            {
                Verbose.W("[ADS] Monetization is not activated or not configured.");
                return false;
            }
            
            var adProviderType = Setting.InterstitialType;
            return IsProviderActive(adProviderType) && _advertisementProviders[adProviderType].IsInterstitialLoaded();
        }

        #endregion

        #region Utils - Private & Internal
        
        internal static void ResetInterstitialDelayTime()
        {
            _lastInterstitialTime = Time.time + Setting.InterstitialInterval;
        }

        internal static void ExecuteInterstitialCallback(bool isSuccess)
        {
            if (_interstitialCallback != null)
            {
                CallEventInMainThread(() => _interstitialCallback.Invoke(isSuccess));
            }
        }

        private static bool ValidateRequestInterstitial(AdProviderType providerType)
        {
            return !IsProviderActive(providerType)
                   || !_advertisementProviders[providerType].IsInitialized
                   || _advertisementProviders[providerType].IsInterstitialLoaded();
        }

        private static bool ValidateShowInterstitial(AdProviderType providerType, bool ignoreCondition)
        {
            return !IsProviderActive(providerType)
                   || (!ignoreCondition && (!ValidateInterstitialTime() || !ValidateInterstitialCondition()))
                   || !_advertisementProviders[providerType].IsInitialized
                   || !_advertisementProviders[providerType].IsInterstitialLoaded();
        }

        private static bool ValidateInterstitialTime()
        {
            Verbose.D("[ADS] ValidateInterstitialTime : " + _lastInterstitialTime + "/ Time : " + Time.time);
            return _lastInterstitialTime < Time.time;
        }
        
        private static bool ValidateInterstitialCondition()
        {
            var conditions = Setting.InterstitialConditions;
            if(conditions == null || conditions.Count == 0)
            {
                return true;
            }

            return Setting.InterstitialConditionType switch
            {
                AdConditionType.AllMatched => conditions.All(customCondition => customCondition.IsMatched()),
                AdConditionType.AnyMatched => conditions.Any(customCondition => customCondition.IsMatched()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion
    }
}