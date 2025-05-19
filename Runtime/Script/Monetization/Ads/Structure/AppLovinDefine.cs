
using System;
using UnityEngine;

namespace ProjectCore.Monetize
{
    [Serializable]
    public class AppLovinDefine
    {
        #region Fields
        
        [Header("Interstitial")]
        [SerializeField] private string _aosInterstitialID = string.Empty;
        [SerializeField] private string _iosInterstitialID = string.Empty;
        
        [Space]
        [Header("Reward")]
        [SerializeField] private string _aosRewardID = string.Empty;
        [SerializeField] private string _iosRewardID = string.Empty;

        [Space]
        [Header("Banner")]
        [SerializeField] private string _aosBannerID = string.Empty;
        [SerializeField] private string _iosBannerID = string.Empty;

        #endregion

        #region Properties
        
#if UNITY_ANDROID
        public string InterstitialID => _aosInterstitialID;
        public string RewardID => _aosRewardID;
        public string BannerID => _aosBannerID;
#elif UNITY_IOS
        public string InterstitialID => _iosInterstitialID;
        public string RewardID => _iosRewardID;
        public string BannerID => _iosBannerID;
#else
        public string InterstitialID => string.Empty;
        public string RewardID => string.Empty;
        public string BannerID => string.Empty;
#endif
        
        #endregion
    }
}