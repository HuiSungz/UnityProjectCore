
using System.Collections.Generic;
using ProjectCore.Attributes;
using UnityEngine;

namespace ProjectCore.Monetize
{
    public sealed class AdsSettingSO : ScriptableObject
    {
        #region Fields

        [BoxGroup("Advertisement")]
        [SerializeField] private AdProviderType _appOpenType = AdProviderType.None;
        [BoxGroup("Advertisement")]
        [SerializeField] private AdProviderType _interstitialType = AdProviderType.None;
        [BoxGroup("Advertisement")]
        [SerializeField] private AdProviderType _rewardedType = AdProviderType.None;
        [BoxGroup("Advertisement")]
        [SerializeField] private AdProviderType _bannerType = AdProviderType.None;
        
        [Space] 
        [BoxGroup("Settings")]
        [Tooltip("Max attempts to load ads")]
        [SerializeField] private int _loadAdsMaxAttempts;
        [BoxGroup("Settings")] 
        [Tooltip("Time out for initialize SDK")]
        [SerializeField] [Range(1f, 30f)] private float _initializeTimeOut = 10f;
        [BoxGroup("Settings")] 
        [Tooltip("Ads delayed call show.")]
        [SerializeField] [Range(0.1f, 3f)] private float _delayInvokeDuration;
        
        [Space]
        [BoxGroup("Interstitial")]
        [Tooltip("초기 게임 런치 시 인터스티셜이 나타나기 전의 시간(초)")]
        [SerializeField] private float _interstitialStartDelay;
        [BoxGroup("Interstitial")]
        [Tooltip("인터스티셜이 나타나는 간격(초), 쿨타임")]
        [SerializeField] private float _interstitialInterval;
        [BoxGroup("Interstitial")]
        [Tooltip("인터스티셜을 자동으로 보여줄 것인지 여부")]
        [SerializeField] private bool _autoShowInterstitial;
        [BoxGroup("Interstitial")] 
        [Tooltip("인터스티셜이 나올 수 있는 추가적인 광고조건")]
        [SerializeField] private List<CustomAdsConditionSO> _interstitialConditions;
        [BoxGroup("Interstitial")] 
        [Tooltip("컨디션 매치 방식 (AND, OR)")]
        [SerializeField] private AdConditionType _interstitialConditionType; 
        
        [BoxGroup("APP LOVIN")]
        [SerializeField] private AppLovinDefine _appLovinDefine;
        [BoxGroup("ADMOB")]
        [SerializeField] private AdMobDefine _adMobDefine;

        #endregion

        #region Properties
        
        public AdProviderType AppOpenType => _appOpenType;
        public AdProviderType InterstitialType => _interstitialType;
        public AdProviderType RewardedType => _rewardedType;
        public AdProviderType BannerType => _bannerType;
        
        public int LoadAdsMaxAttempts => _loadAdsMaxAttempts;
        public float InitializeTimeOut => _initializeTimeOut;
        public float DelayInvokeDuration => _delayInvokeDuration;
        
        public float InterstitialStartDelay => _interstitialStartDelay;
        public float InterstitialInterval => _interstitialInterval;
        public bool AutoShowInterstitial => _autoShowInterstitial;
        public IReadOnlyList<CustomAdsConditionSO> InterstitialConditions => _interstitialConditions;
        public AdConditionType InterstitialConditionType => _interstitialConditionType;
        
        public AppLovinDefine AppLovinDefine => _appLovinDefine;
        public AdMobDefine AdMobDefine => _adMobDefine;

        #endregion
    }
}