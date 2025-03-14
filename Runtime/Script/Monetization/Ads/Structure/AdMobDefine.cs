

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Monetize
{
    [Serializable]
    public class AdMobDefine
    {
        #region Fields
        
        [Header("App Open ID")]
        [SerializeField] private string _androidAppOpenID;
        [SerializeField] private string _iosAppOpenID;
        
        [Space]
        [Header("App Open Ad Interval (Seconds)")]
        [SerializeField] private int _appOpenAdInterval = 60;
        [SerializeField] private List<CustomAdsConditionSO> _appOpenConditions;
        [SerializeField] private AdConditionType _appOpenConditionType;

        #endregion

        #region Properties
        
        public string AndroidAppOpenID => _androidAppOpenID;
        public string IosAppOpenID => _iosAppOpenID;
        
        public int AppOpenAdInterval => _appOpenAdInterval;
        public IReadOnlyList<CustomAdsConditionSO> AppOpenConditions => _appOpenConditions;
        public AdConditionType AppOpenConditionType => _appOpenConditionType;

        #endregion
    }
}