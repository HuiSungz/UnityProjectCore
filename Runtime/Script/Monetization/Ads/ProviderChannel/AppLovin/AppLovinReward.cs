
using System;
using ProjectCore.Utilities;

namespace ProjectCore.Monetize
{
#if SDK_INSTALLED_APPLOVINMAX
    internal sealed class AppLovinReward : BaseAppLovinAdvertisement
    {
        private bool _hasReceivedReward;

        internal AppLovinReward(MonetizationSettingSO setting) : base(setting)
        {
            _hasReceivedReward = false;
        }

        public override void RegisterEvents()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnFailed;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnShowSuccess;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnReceived;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnShowFailed;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        }

        public override void UnregisterEvents()
        {
            SafeCancelAndDispose();
            
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnFailed;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnShowSuccess;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= OnReceived;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnShowFailed;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnAdRevenuePaidEvent;
        }

        public override bool IsLoad(string unitId)
        {
            return MaxSdk.IsRewardedAdReady(unitId);
        }

        public override void Show(string unitId, Action successAction, Action failureAction)
        {
            try
            {
                _successCallback = successAction;
                _failureCallback = failureAction;
                MaxSdk.SetMuted(true);
                MaxSdk.ShowRewardedAd(unitId);
            }
            catch (Exception ex)
            {
                Verbose.Ex("[AppLovinReward] 리워드 광고 표시 중 오류 발생", ex);
                _failureCallback?.Invoke();
            }
        }

        protected override void LoadAd(string unitId)
        {
            MaxSdk.LoadRewardedAd(unitId);
        }

        protected override void OnLoaded(string unitId, MaxSdkBase.AdInfo adInfo)
        {
            _isLoading = false;
            _loadAttempts = 0;
        }

        protected override void OnFailed(string unitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            HandleAdFailure(unitId, errorInfo, "AppLovinReward");
        }
        
        private void OnReceived(string unitId, MaxSdkBase.Reward rewardInfo, MaxSdkBase.AdInfo adInfo)
        {
            _hasReceivedReward = true;
        }

        protected override void OnShowSuccess(string unitId, MaxSdkBase.AdInfo adInfo)
        {
            if (_hasReceivedReward)
            {
                _successCallback?.Invoke();
                _hasReceivedReward = false;
            }
            else
            {
                _failureCallback?.Invoke();
            }
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