
using System;
using System.Linq;
using ProjectCore.Utilities;
using UnityEngine;

namespace ProjectCore.Monetize
{
    public static partial class ADS
    {
        #region Fields

        private static float _lastAppOpenTime;
        internal static bool AppOpenAvailable { get; set; } = true;

        #endregion
        
        public static void ShowAppOpen()
        {
            if (!Monetization.IsActivate || !_isConfigure)
            {
                Verbose.W("[ADS] Monetization is not activated or not configured.");
                return;
            }

            var adProvideType = Setting.AppOpenType;
            if (ValidateShowAppOpen(adProvideType))
            {
                Verbose.W("[ADS] Can't show App Open.");
                return;
            }
            
            _advertisementProviders[adProvideType].ShowAppOpen();
        }

        #region Utils - Private & Internal

        internal static void ResetAppOpenDelayTime()
        {
            _lastAppOpenTime = Time.time + Setting.AdMobDefine.AppOpenAdInterval;
        }

        private static bool ValidateAppOpenTime()
        {
            Verbose.D("[ADS] ValidateAppOpenTime : " + _lastAppOpenTime + "/ Time : " + Time.time);
            return _lastAppOpenTime < Time.time;
        }

        private static bool ValidateAppOpenCondition()
        {
            var conditions = Setting.AdMobDefine.AppOpenConditions;
            if (conditions == null || conditions.Count == 0)
            {
                return true;
            }

            return Setting.AdMobDefine.AppOpenConditionType switch
            {
                AdConditionType.AllMatched => conditions.All(condition => condition.IsMatched()),
                AdConditionType.AnyMatched => conditions.Any(condition => condition.IsMatched()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static bool ValidateShowAppOpen(AdProviderType adProviderType)
        {
            return !IsProviderActive(adProviderType)
                   || !ValidateAppOpenTime() || !ValidateAppOpenCondition()
                   || !_advertisementProviders[adProviderType].IsInitialized
                   || !AppOpenAvailable;
        }

        #endregion
    }
}