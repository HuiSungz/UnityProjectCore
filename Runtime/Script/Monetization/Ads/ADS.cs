
using System.Collections.Generic;
using ProjectCore.Utilities;

namespace ProjectCore.Monetize
{
    public static partial class ADS
    {
        #region Fields & Properties
        
        private static AdProvider[] _adProviders;
        private static AdsSettingSO _setting;
        
        private static Dictionary<AdProviderType, AdProvider> _advertisementProviders;
        private static bool _isConfigure;
        
        internal static AdsSettingSO Setting => _setting;

        #endregion

        #region Initialize

        public static void Configure(MonetizationSettingSO monetizationSetting)
        {
            if (_isConfigure)
            {
                Verbose.W("[ADS] already configured");
                return;
            }

            _isConfigure = true;
            _setting = monetizationSetting.AdsSetting;
            
            if (!_setting)
            {
                Verbose.E("[ADS] AdsSettingSO is null");
                return;
            }

            InitializeInternal(monetizationSetting);
        }

        #endregion

        #region Ads Provider

        private static AdProvider[] GetAdProviders()
        {
            return new AdProvider[]
            {
#if SDK_INSTALLED_APPLOVINMAX
                new AppLovinProvider(AdProviderType.AppLovin),
#endif

#if SDK_INSTALLED_ADMOB
                new AdMobProvider(AdProviderType.AdMob),
#endif
            };
        }

        #region Provider Utils

        public static bool IsProviderEnabled(AdProviderType providerType)
        {
            if (!Monetization.IsActivate || !_isConfigure)
            {
                return false;
            }
            
            return Setting.BannerType == providerType 
                   || Setting.InterstitialType == providerType 
                   || Setting.RewardedType == providerType
                   || Setting.AppOpenType == providerType;
        }
        
        public static bool IsProviderActive(AdProviderType providerType)
        {
            return _advertisementProviders.ContainsKey(providerType);
        }
        
        public static bool IsProviderInitialized(AdProviderType providerType)
        {
            return _advertisementProviders.TryGetValue(providerType, out var provider) && provider.IsInitialized;
        }

        #endregion

        #endregion
    }
}