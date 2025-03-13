
using Cysharp.Threading.Tasks;

namespace ProjectCore.Monetize
{
    public abstract class AdProvider
    {
        #region Fields & Constructor

        protected MonetizationSettingSO MonetizationSetting;
        protected AdsSettingSO AdsSetting;

        public AdProviderType ProviderType { get; protected set; }
        public bool IsInitialized { get; private set; }

        protected AdProvider(AdProviderType providerType)
        {
            ProviderType = providerType;
        }

        #endregion

        #region Main Methods

        public void LinkSettings(MonetizationSettingSO monetizationSetting)
        {
            MonetizationSetting = monetizationSetting;
            AdsSetting = monetizationSetting.AdsSetting;
        }

        public async UniTask<bool> InitializeAsync()
        {
            if (IsInitialized)
            {
                return true;
            }
            
            var initResult = await InitializeProviderAsync();
            if(!initResult)
            {
                return false;
            }

            IsInitialized = true;
            ADSCallback.RaiseAdProviderInitialized(ProviderType);

            return true;
        }

        #endregion

        #region Abstract

        protected abstract UniTask<bool> InitializeProviderAsync();
        
        public abstract void ShowBanner();
        public abstract void HideBanner();
        public abstract void DestroyBanner();

        public abstract void RequestInterstitial();
        public abstract void ShowInterstitial(AdvertisementCallback callback);
        public abstract bool IsInterstitialLoaded();

        public abstract void RequestRewardedVideo();
        public abstract void ShowRewardedVideo(AdvertisementCallback callback);
        public abstract bool IsRewardedVideoLoaded();
        
        public delegate void AdvertisementCallback(bool result);

        #endregion
    }
}