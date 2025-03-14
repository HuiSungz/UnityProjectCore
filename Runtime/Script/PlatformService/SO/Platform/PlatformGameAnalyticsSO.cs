
#if SDK_INSTALLED_GAMEANALYTICS
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameAnalyticsSDK;
using UnityEngine;

namespace ProjectCore.PlatformService
{
    internal sealed class PlatformGameAnalyticsSO : PlatformSingletonSO<PlatformGameAnalyticsSO>, IPlatformInitializer
    {
        #region Fields

        public override bool Initialized => GameAnalytics.Initialized;

        private Action _onInitializeComplete;
        private CancellationTokenSource _initializeCts;
        private bool _isInitializing;

        #endregion
        
        public void Initialize(Action onInitializeComplete = null)
        {
            if(IsInitialized)
            {
                return;
            }
            
            if(!Ref)
            {
                Debug.LogError("PlatformGameAnalyticsSO 인스턴스를 찾거나 생성할 수 없습니다.");
                return;
            }
            
            _onInitializeComplete = onInitializeComplete;
            InitializeAsync().Forget();
        }
        
        private async UniTask InitializeAsync()
        {
            if (_isInitializing)
            {
                return;
            }
            
            _isInitializing = true;
            _initializeCts = new CancellationTokenSource();
            
            try
            {
                var prefab = Resources.Load<GameObject>("Platform/GameAnalytics");
                if (!prefab)
                {
                    Debug.LogError("Platform/GameAnalytics 프리팹을 찾을 수 없습니다.");
                    _isInitializing = false;
                    return;
                }
                
                var gameAnalyticsObject = Instantiate(prefab);
                gameAnalyticsObject.name = "[Platform] GameAnalytics";

                if (!gameAnalyticsObject.TryGetComponent<GameAnalytics>(out var _))
                {
                    Debug.LogError("GameAnalytics 컴포넌트를 찾을 수 없습니다.");
                    _isInitializing = false;
                    return;
                }

                await UniTask.NextFrame();
                
                GameAnalytics.Initialize();
                
                await UniTask.WaitUntil(() => Initialized, cancellationToken: _initializeCts.Token);
                IsInitialized = true;
                _onInitializeComplete?.Invoke();
                _onInitializeComplete = null;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Singular 초기화가 취소되었습니다.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Singular 초기화 중 오류 발생: {e.Message}");
            }
            finally
            {
                _isInitializing = false;
                _initializeCts?.Dispose();
                _initializeCts = null;
            }
        }
        
        private void CancelInitialization()
        {
            if (_isInitializing && _initializeCts is { IsCancellationRequested: false })
            {
                _initializeCts.Cancel();
            }
        }
        
        private void OnDestroy()
        {
            CancelInitialization();
        }
    }
}
#endif