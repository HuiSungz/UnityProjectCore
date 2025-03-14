
using System;
using ProjectCore.Utilities;

namespace ProjectCore.Monetize
{
#if SDK_INSTALLED_APPLOVINMAX
    internal sealed class AppLovinInterstitial : BaseAppLovinAdvertisement
    {
        internal AppLovinInterstitial(MonetizationSettingSO setting) : base(setting) { }

        public override void RegisterEvents()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnFailed;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnShowSuccess;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnShowFailed;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        }

        public override void UnregisterEvents()
        {
            SafeCancelAndDispose();
            
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnFailed;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnShowSuccess;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnShowFailed;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnAdRevenuePaidEvent;
        }

        public override bool IsLoad(string unitId)
        {
            return MaxSdk.IsInterstitialReady(unitId);
        }

        public override void Show(string unitId, Action successAction, Action failureAction)
        {
            try
            {
                _successCallback = successAction;
                _failureCallback = failureAction;
                MaxSdk.SetMuted(true);
                MaxSdk.ShowInterstitial(unitId);
            }
            catch (Exception ex)
            {
                Verbose.Ex("[AppLovinInterstitial] 인터스티셜 광고 표시 중 오류 발생", ex);
                _failureCallback?.Invoke();
            }
        }

        protected override void LoadAd(string unitId)
        {
            MaxSdk.LoadInterstitial(unitId);
        }

        protected override void OnLoaded(string unitId, MaxSdkBase.AdInfo adInfo)
        {
            _isLoading = false;
            _loadAttempts = 0;
        }

        protected override void OnFailed(string unitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            HandleAdFailure(unitId, errorInfo, "AppLovinInterstitial");
        }

        protected override void OnShowSuccess(string unitId, MaxSdkBase.AdInfo adInfo)
        {
            _successCallback?.Invoke();
        }

        protected override void OnShowFailed(string unitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            _failureCallback?.Invoke();
        }

        protected override void OnAdRevenuePaidEvent(string unitId, MaxSdkBase.AdInfo adInfo)
        {
            // TODO Analytics
        }
    }
#endif
}