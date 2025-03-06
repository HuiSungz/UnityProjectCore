
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectCore.Preference
{
    internal static class PreferenceController
    {
        #region Fields

        private static Dictionary<string, PreferenceService> _preferenceMap;
        private static Dictionary<string, bool> _isDataLoadedMap;
        private static Dictionary<string, bool> _isSaveRequiredMap;
        private static Dictionary<Type, string> _typeToPreferenceMap;
        
        private static bool _isDataLoaded;
        private static bool _isSaveRequired;
        private static bool _isProcessingSave;

        private static PreferenceSettingSO _setting;

        public static event Action<string> OnDataLoaded;

        #endregion

        #region Get Preference

        /// <summary>
        /// 지정된 파일명과 고유 키로 환경설정 객체를 가져옵니다. (기존 호환성 유지)
        /// </summary>
        /// <typeparam name="T">환경설정 객체 타입</typeparam>
        /// <param name="fileName">파일명</param>
        /// <param name="uniqueKey">고유 키</param>
        /// <returns>환경설정 객체</returns>
        public static T GetPreferenceObject<T>(string fileName, string uniqueKey) where T : IPreferenceObject, new()
        {
            if (!_preferenceMap.ContainsKey(fileName))
            {
                Debug.LogError($"Preference file '{fileName}' is not registered.");
                return default;
            }

            if (!_isDataLoadedMap[fileName])
            {
                Load(fileName);
            }

            return _preferenceMap[fileName].GetPreferenceObject<T>(uniqueKey.GetHashCode());
        }

        /// <summary>
        /// 고유 키로 환경설정 객체를 가져옵니다. 파일명은 객체 타입에서 자동으로 결정됩니다.
        /// </summary>
        /// <typeparam name="T">환경설정 객체 타입</typeparam>
        /// <param name="uniqueKey">고유 키</param>
        /// <returns>환경설정 객체</returns>
        public static T GetPreferenceObject<T>(string uniqueKey) where T : IPreferenceObject, new()
        {
            var preferenceType = typeof(T);
            if (_typeToPreferenceMap.TryGetValue(preferenceType, out var fileName))
            {
                return GetPreferenceObject<T>(fileName, uniqueKey);
            }
            
            var tempObj = new T();
            fileName = tempObj.AssociatedPreferenceKey;
            _typeToPreferenceMap[preferenceType] = fileName;
            
            return GetPreferenceObject<T>(fileName, uniqueKey);
        }

        #endregion
        
        #region Initialize

        public static void Initialize(PreferenceSettingSO setting)
        {
            _setting = setting;
            
            PreferenceIO.Initialize();
            
            _preferenceMap = new Dictionary<string, PreferenceService>();
            _isDataLoadedMap = new Dictionary<string, bool>();
            _isSaveRequiredMap = new Dictionary<string, bool>();
            _typeToPreferenceMap = new Dictionary<Type, string>();

            InitializeInternal();
            ValidateAutoSave(CreateUnityCallbackReceiver());
        }

        private static void InitializeInternal()
        {
            foreach (var preferenceFileInfo in _setting.PreferenceFileInfos)
            {
                var fileName = preferenceFileInfo.PreferenceFileName;
                if (_preferenceMap.ContainsKey(fileName))
                {
                    Debug.LogWarning("Preference file name is duplicated: " + fileName);
                    continue;
                }

                var preferenceService = new PreferenceService
                {
                    Info = preferenceFileInfo
                };
                _preferenceMap.Add(fileName, preferenceService);
                _isDataLoadedMap.Add(fileName, false);
                _isSaveRequiredMap.Add(fileName, false);

                if (_setting.ClearOnSaves)
                {
                    InitClearAllPreferenceService();
                }
                else if(_setting.AutoLoad)
                {
                    LoadAll();
                }
            }
        }

        private static UnityCallbackReceiver CreateUnityCallbackReceiver()
        {
            var saveCallbackReceiver = new GameObject("[SAVE CALLBACK RECEIVER]")
            {
                hideFlags = HideFlags.HideInHierarchy
            };
            
            Object.DontDestroyOnLoad(saveCallbackReceiver);
            var unityCallbackReceiver = saveCallbackReceiver.AddComponent<UnityCallbackReceiver>();
            return unityCallbackReceiver;
        }

        private static void ValidateAutoSave(UnityCallbackReceiver unityCallbackReceiver)
        {
            if (_setting.AutoSaveInterval > 0)
            {
                unityCallbackReceiver.StartCoroutine(AutoSaveCoroutine(_setting.AutoSaveInterval));
            }
        }

        #endregion

        #region Load

        public static void Load(string fileName)
        {
            if (!_preferenceMap.ContainsKey(fileName))
            {
                Debug.LogError($"Preference file name is not found: {fileName}");
                return;
            }

            if (_isDataLoadedMap[fileName])
            {
                return;
            }
            
            var preferenceService = _preferenceMap[fileName];
            var loadedFile = PreferenceIO.Deserialize(preferenceService.Info);

            if (loadedFile != null)
            {
                _preferenceMap[fileName] = loadedFile;
                loadedFile.Initialize();
            }
            else
            {
                preferenceService.Initialize();
            }

            _isDataLoadedMap[fileName] = true;
            
            OnDataLoaded?.Invoke(fileName);
        }

        public static void LoadAll()
        {
            var fileNames = new List<string>(_preferenceMap.Keys);
            foreach(var fileName in fileNames)
            {
                Load(fileName);
            }
        }

        #endregion

        #region Save

        public static void Save(string fileName, bool forceSave = false)
        {
            if (!_preferenceMap.ContainsKey(fileName))
            {
                Debug.LogError($"Preference file name is not found: {fileName}");
                return;
            }

            if (!forceSave && !_isSaveRequiredMap[fileName])
            {
                return;
            }
            
            var preferenceService = _preferenceMap[fileName];
            preferenceService.Flush();

            if (_setting.SaveUseThread)
            {
                var thread = new Thread(() => PreferenceIO.Serialize(preferenceService));
                thread.Start();
            }
            else
            {
                PreferenceIO.Serialize(preferenceService);
            }
            
            _isSaveRequiredMap[fileName] = false;
        }
        
        public static void SaveAll(bool forceSave = false)
        {
            var fileNames = new List<string>(_preferenceMap.Keys);
            foreach (var fileName in fileNames)
            {
                Save(fileName, forceSave);
            }
        }

        private static IEnumerator AutoSaveCoroutine(float saveDelay)
        {
            var waitForSeconds = new WaitForSeconds(saveDelay);

            while (true)
            {
                yield return waitForSeconds;

                SaveAll();
            }
            
            // ReSharper disable once IteratorNeverReturns
        }

        #endregion

        #region Utility

        private static void InitClearPreferenceService(string fileName)
        {
            if (!_preferenceMap.ContainsKey(fileName))
            {
                return;
            }

            _preferenceMap[fileName].Initialize();
            _isDataLoadedMap[fileName] = true;
            
            Debug.Log("[PreferenceController]: Preference file is cleared: " + fileName);
        }
        
        private static void InitClearAllPreferenceService()
        {
            var fileNames = new List<string>(_preferenceMap.Keys);
            foreach (var fileName in fileNames)
            {
                InitClearPreferenceService(fileName);
            }
        }
        
        public static void MarkAsSaveIsRequired(string fileName)
        {
            if (_preferenceMap.ContainsKey(fileName))
            {
                _isSaveRequiredMap[fileName] = true;
            }
        }

        #endregion
        
        private class UnityCallbackReceiver : MonoBehaviour
        {
            private void OnDestroy()
            {
#if UNITY_EDITOR
                SaveAll(true);
#endif
            }

            private void OnApplicationFocus(bool hasFocus)
            {
#if !UNITY_EDITOR
                if (!hasFocus)
                {
                    SaveAll();
                }
#endif
            }
        }
    }
}