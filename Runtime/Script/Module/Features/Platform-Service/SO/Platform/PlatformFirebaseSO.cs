
#if SDK_INSTALLED_FIREBASE
using System;
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.Extensions;
using UnityEngine;

namespace ProjectCore.Module
{
    internal sealed class PlatformFirebaseSO : PlatformSingletonSO<PlatformFirebaseSO>, IPlatformInitializer
    {
        #region Fields
        
        [SerializeField] private bool _useCrashlytics = true;
        [SerializeField] private bool _useAnalytics = true;

        private FirebaseApp _app;
        public FirebaseApp App => _app;
        
        private Action _onInitializeComplete;

        #endregion

        public void Initialize(Action onInitializeComplete = null)
        {
            if (IsInitialized)
            {
                return;
            }
            
            if(!Ref)
            {
                Debug.LogError("PlatformFirebaseSO 인스턴스를 찾거나 생성할 수 없습니다.");
                return;
            }
            
            _onInitializeComplete = onInitializeComplete;
            InitializeInternal();
        }

        private void InitializeInternal()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    _app = FirebaseApp.DefaultInstance;

                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(_useAnalytics);
                    Crashlytics.IsCrashlyticsCollectionEnabled = _useCrashlytics;
                    Crashlytics.ReportUncaughtExceptionsAsFatal = _useCrashlytics;

                    IsInitialized = true;
                    _onInitializeComplete?.Invoke();
                    _onInitializeComplete = null;
                }
                else
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
        }
    }
}
#endif