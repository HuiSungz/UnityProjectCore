

using System;
using UnityEngine;

namespace ProjectCore.Monetize
{
    [Serializable]
    public class AdMobDefine
    {
        #region Fields

        [Header("App ID")]
        [SerializeField] private string _androidAppID;
        [SerializeField] private string _iosAppID;

        [Space]
        [Header("App Open ID")]
        [SerializeField] private string _androidAppOpenID;
        [SerializeField] private string _iosAppOpenID;
        
        [Space]
        [Header("App Open Ad Interval (Seconds)")]
        [SerializeField] private int _appOpenAdInterval = 60;

        #endregion

        #region Properties
        
        public string AndroidAppID => _androidAppID;
        public string IosAppID => _iosAppID;
        
        public string AndroidAppOpenID => _androidAppOpenID;
        public string IosAppOpenID => _iosAppOpenID;
        
        public int AppOpenAdInterval => _appOpenAdInterval;

        #endregion
    }
}