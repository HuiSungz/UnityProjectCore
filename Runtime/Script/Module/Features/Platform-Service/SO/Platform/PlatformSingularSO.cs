
#if SDK_INSTALLED_SINGULAR
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Singular;
using UnityEngine;

namespace ProjectCore.Module
{
    internal sealed class PlatformSingularSO : PlatformSingletonSO<PlatformSingularSO>, IPlatformInitializer
    {
        #region Fields

        [SerializeField] private string ApiKey;
        [SerializeField] private string ApiSecretKey;
        [SerializeField] private bool _isVerboseLog = true;
        [SerializeField] private int _logLevel = 3;

        public override bool Initialized
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return SingularSDK.Initialized;
#endif
            }
        }

        private Action _onInitializeComplete;
        private CancellationTokenSource _initializeCts;
        private bool _isInitializing;

        #endregion
        
        public void Initialize(Action onInitializeComplete = null)
        {
            if (IsInitialized)
            {
                onInitializeComplete?.Invoke();
                return;
            }
            
            if(!Ref)
            {
                Debug.LogError("PlatformSingularSO 인스턴스를 찾거나 생성할 수 없습니다.");
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
                var prefab = Resources.Load<GameObject>("Platform/Singular");
                if (!prefab)
                {
                    Debug.LogError("Platform/Singular 프리팹을 찾을 수 없습니다.");
                    _isInitializing = false;
                    return;
                }
                
                var singularObject = Instantiate(prefab);
                singularObject.name = "[Platform] Singular";

                if (!singularObject.TryGetComponent<SingularSDK>(out var singularSdk))
                {
                    Debug.LogError("SingularSDK 컴포넌트를 찾을 수 없습니다.");
                    _isInitializing = false;
                    return;
                }
                
                singularSdk.SingularAPIKey = ApiKey;
                singularSdk.SingularAPISecret = ApiSecretKey;
                singularSdk.enableLogging = _isVerboseLog;
                singularSdk.logLevel = _logLevel;
                
                SingularSDK.InitializeSingularSDK();
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