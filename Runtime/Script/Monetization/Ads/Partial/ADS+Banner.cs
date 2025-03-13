
using ProjectCore.Utilities;

namespace ProjectCore.Monetize
{
    public static partial class ADS
    {
        #region Fields

        private static bool _isBannerActive;

        #endregion

        public static void ShowBanner()
        {
            if (!Monetization.IsActivate || !_isConfigure)
            {
                Verbose.W("[ADS] Monetization is not activated or not configured.");
                return;
            }

            if (!_isBannerActive)
            {
                return;
            }
            
            var adProviderType = Setting.BannerType;
            if (ValidateBanner(adProviderType))
            {
                return;
            }
            
            _advertisementProviders[adProviderType].ShowBanner();
        }

        public static void DestroyBanner()
        {
            if (!Monetization.IsActivate || !_isConfigure)
            {
                Verbose.W("[ADS] Monetization is not activated or not configured.");
                return;
            }
            
            var adProviderType = Setting.BannerType;
            if (ValidateBanner(adProviderType))
            {
                return;
            }
            
            _advertisementProviders[adProviderType].DestroyBanner();
        }

        public static void HideBanner()
        {
            if (!Monetization.IsActivate || !_isConfigure)
            {
                Verbose.W("[ADS] Monetization is not activated or not configured.");
                return;
            }
            
            var adProviderType = Setting.BannerType;
            if (ValidateBanner(adProviderType))
            {
                return;
            }
            
            _advertisementProviders[adProviderType].HideBanner();
        }

        public static void EnableBanner()
        {
            if (!Monetization.IsActivate || !_isConfigure)
            {
                Verbose.W("[ADS] Monetization is not activated or not configured.");
                return;
            }

            _isBannerActive = true;
            ShowBanner();
        }
        
        public static void DisableBanner()
        {
            if (!Monetization.IsActivate || !_isConfigure)
            {
                Verbose.W("[ADS] Monetization is not activated or not configured.");
                return;
            }

            _isBannerActive = false;
            HideBanner();
        }

        #region Utils - Private

        private static bool ValidateBanner(AdProviderType providerType)
        {
            return !IsProviderActive(providerType) || !_advertisementProviders[providerType].IsInitialized;
        }

        #endregion
    }
}