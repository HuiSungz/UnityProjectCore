
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NetworkConnector;
using ProjectCore.Utilities;
using UnityEngine;

namespace ProjectCore.Monetize
{
#if SDK_INSTALLED_APPLOVINMAX
    internal abstract class BaseAppLovinAdvertisement : IAppLovin
    {
        #region Fields

        protected readonly MonetizationSettingSO _setting;
        protected CancellationTokenSource _cts;
        
        protected bool _isLoading;
        protected int _loadAttempts;
        protected bool _isNetworkOnce;
        protected bool _isDisposed;

        protected Action _successCallback;
        protected Action _failureCallback;
        
        protected BaseAppLovinAdvertisement(MonetizationSettingSO setting)
        {
            _setting = setting;
            _loadAttempts = 0;
            _isNetworkOnce = false;
            _isDisposed = false;
            _cts = new CancellationTokenSource();
        }

        #endregion

        public abstract void RegisterEvents();
        public abstract void UnregisterEvents();
        public abstract bool IsLoad(string unitId);
        public abstract void Show(string unitId, Action successAction, Action failureAction);
        
        protected abstract void LoadAd(string unitId);
        protected abstract void OnLoaded(string unitId, MaxSdkBase.AdInfo adInfo);
        protected abstract void OnFailed(string unitId, MaxSdkBase.ErrorInfo errorInfo);
        protected abstract void OnShowSuccess(string unitId, MaxSdkBase.AdInfo adInfo);
        protected abstract void OnShowFailed(string unitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo);
        protected abstract void OnAdRevenuePaidEvent(string unitId, MaxSdkBase.AdInfo adInfo);
        
        public void Load(string unitId)
        {
            if (_isLoading)
            {
                return;
            }
            
            SafeCancelAndDispose();
            _cts = new CancellationTokenSource();
            _isDisposed = false;
            
            _loadAttempts = 0;

            if (_setting.UseNetworkValidate)
            {
                LoadWithNetworkValidation(unitId).Forget();
            }
            else
            {
                _isLoading = true;
                LoadAd(unitId);
            }
        }
        
        protected void SafeCancelAndDispose()
        {
            if (_cts == null || _isDisposed)
            {
                return;
            }
            
            try
            {
                _cts.Cancel();
                _cts.Dispose();
                _isDisposed = true;
            }
            catch (ObjectDisposedException ex)
            {
                Verbose.Ex($"[{GetType().Name}] CancellationTokenSource가 이미 폐기됨", ex);
            }
            catch (Exception ex)
            {
                Verbose.Ex($"[{GetType().Name}] CancellationTokenSource 취소/폐기 중 오류 발생", ex);
            }
            finally
            {
                _cts = null;
            }
        }

        protected async UniTask LoadWithNetworkValidation(string unitId)
        {
            try
            {
                var isConnected = await NConnector.ValidationConnectionAsync(NetworkValidationType.All);
                if (isConnected)
                {
                    _isLoading = true;
                    _isNetworkOnce = false;
                    LoadAd(unitId);
                }
                else
                {
                    _isNetworkOnce = true;
                    Verbose.W($"[{GetType().Name}] 네트워크 연결 실패로 광고 로드를 건너뜁니다.");
                }
            }
            catch (OperationCanceledException)
            {
                // 작업이 취소됨
            }
        }

        protected void HandleAdFailure(string unitId, MaxSdkBase.ErrorInfo errorInfo, string typeName)
        {
            _isLoading = false;
            if (_setting.UseNetworkValidate && _isNetworkOnce)
            {
                Verbose.W($"[{typeName}] 네트워크 연결 문제로 인해 광고 로드 재시도를 중단합니다.");
                return;
            }
            
            var adsSetting = _setting.AdsSetting;
            _loadAttempts++;
            if (_loadAttempts <= adsSetting.LoadAdsMaxAttempts)
            {
                var retryDelay = Mathf.Pow(2, _loadAttempts - 1);
                
                Verbose.D($"[{typeName}] 광고 로드 실패({_loadAttempts}/{adsSetting.LoadAdsMaxAttempts})," +
                          $" {retryDelay}초 후 재시도: {errorInfo.Message}");
                
                if (_cts != null && !_isDisposed)
                {
                    RetryLoadAfterDelay(unitId, retryDelay, _cts.Token).Forget();
                }
                else
                {
                    Verbose.W($"[{typeName}] CancellationTokenSource가 유효하지 않아 재시도할 수 없습니다.");
                }
            }
            else
            {
                Verbose.W($"[{typeName}] 광고 로드 최대 시도 횟수({adsSetting.LoadAdsMaxAttempts})를 초과했습니다.");
            }
        }
        
        protected async UniTask RetryLoadAfterDelay(string unitId, float delay, CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                if (_setting.UseNetworkValidate)
                {
                    var isConnected = await NConnector.ValidationConnectionAsync(NetworkValidationType.All);
                    if (isConnected)
                    {
                        _isLoading = true;
                        _isNetworkOnce = false;
                        LoadAd(unitId);
                    }
                    else
                    {
                        _isNetworkOnce = true;
                        Verbose.W($"[{GetType().Name}] 네트워크 연결 실패로 광고 로드 재시도를 건너뜁니다.");
                    }
                }
                else
                {
                    _isLoading = true;
                    LoadAd(unitId);
                }
            }
            catch (OperationCanceledException)
            {
                // 작업이 취소됨
            }
        }
    }
#endif
}