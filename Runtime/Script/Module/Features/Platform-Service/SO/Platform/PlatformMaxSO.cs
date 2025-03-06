
#if SDK_INSTALLED_APPLOVINMAX
using System;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace ProjectCore.Module
{
    internal sealed class PlatformMaxSO : PlatformSingletonSO<PlatformMaxSO>, IPlatformInitializer
    {
        #region Fields

        [SerializeField] private bool _isVerboseLog;
        [SerializeField] private bool _isInitGdpr;
        
        [Space]
        [SerializedDictionary("Device Owner", "Device Ad-ID")]
        [SerializeField] private SerializedDictionary<string, string> _testDeviceMap;
        [SerializeField] private TextAsset _testDeviceCsv;
        
        private const string MaxParameterName = "disable_all_logs";
        private const string MaxParameterValue = "true";
        
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
                Debug.LogError("PlatformMaxSO 인스턴스를 찾거나 생성할 수 없습니다.");
                return;
            }

            _onInitializeComplete = onInitializeComplete;
            InitializeInternal();
        }

        private void InitializeInternal()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += configuration =>
            {
                if (_isInitGdpr)
                {
                    InitializeGdpr(configuration);
                }
                
                IsInitialized = true;
                _onInitializeComplete?.Invoke();
                _onInitializeComplete = null;
            };

            if (_isVerboseLog)
            {
                MaxSdk.SetVerboseLogging(true);
            }
            else
            {
                MaxSdk.SetVerboseLogging(false);
                MaxSdk.SetExtraParameter(MaxParameterName, MaxParameterValue);
            }

            MaxSdk.SetTestDeviceAdvertisingIdentifiers(_testDeviceMap.Values.ToArray());
            MaxSdk.InitializeSdk();
        }

        private void InitializeGdpr(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            if (sdkConfiguration.ConsentFlowUserGeography != MaxSdkBase.ConsentFlowUserGeography.Gdpr)
            {
                return;
            }
            
            var cmpService = MaxSdk.CmpService;
            cmpService.ShowCmpForExistingUser(error =>
            {
                if (error == null)
                {
                    Debug.Log("CMP dialog shown successfully.");
                }
                else
                {
                    Debug.LogError("Failed to show CMP dialog: " + error.Message);
                }
            });
        }
    }
}
#endif