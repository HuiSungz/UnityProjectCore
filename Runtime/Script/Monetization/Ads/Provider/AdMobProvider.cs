
using Cysharp.Threading.Tasks;

namespace ProjectCore.Monetize
{
#if SDK_INSTALLED_ADMOB
    public class AdMobProvider : AdProvider
    {
        public AdMobProvider(AdProviderType providerType) : base(providerType)
        {
        }

        protected async override UniTask<bool> InitializeProviderAsync()
        {
            return true;
        }

        public override void ShowBanner()
        {
            throw new System.NotImplementedException();
        }

        public override void HideBanner()
        {
            throw new System.NotImplementedException();
        }

        public override void DestroyBanner()
        {
            throw new System.NotImplementedException();
        }

        public override void RequestInterstitial()
        {
            throw new System.NotImplementedException();
        }

        public override void ShowInterstitial(AdvertisementCallback callback)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsInterstitialLoaded()
        {
            throw new System.NotImplementedException();
        }

        public override void RequestRewardedVideo()
        {
            throw new System.NotImplementedException();
        }

        public override void ShowRewardedVideo(AdvertisementCallback callback)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsRewardedVideoLoaded()
        {
            throw new System.NotImplementedException();
        }
    }
#endif
}