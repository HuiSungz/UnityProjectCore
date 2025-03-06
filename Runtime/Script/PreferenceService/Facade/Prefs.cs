
using System;

using UnityEngine;

namespace ProjectCore.Preference
{
    public static class Prefs
    {
        private static bool _isInitialized;
        public static bool Initialized => _isInitialized;
        
        #region Initialize

        public static void Initialize(PreferenceSettingSO preferenceSetting)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("PreferenceService is already initialized.");
                return;
            }
            
            PreferenceController.Initialize(preferenceSetting);
            
            _isInitialized = true;
        }

        #endregion
        
        #region Events
        
        /// <summary>
        /// 데이터가 로드될 때 발생하는 이벤트를 구독합니다.
        /// </summary>
        /// <param name="onDataLoaded">데이터 로드 시 호출될 콜백</param>
        public static void SubscribeToDataLoaded(Action<string> onDataLoaded)
        {
            PreferenceController.OnDataLoaded += onDataLoaded;
        }

        /// <summary>
        /// 데이터가 로드될 때 발생하는 이벤트 구독을 해제합니다.
        /// </summary>
        /// <param name="onDataLoaded">제거할 콜백</param>
        public static void UnsubscribeFromDataLoaded(Action<string> onDataLoaded)
        {
            PreferenceController.OnDataLoaded -= onDataLoaded;
        }

        #endregion

        #region Get

        /// <summary>
        /// 고유 키를 사용하여 데이터 객체를 가져옵니다.
        /// </summary>
        /// <typeparam name="T">데이터 객체 타입</typeparam>
        /// <param name="uniqueKey">고유 키</param>
        /// <returns>데이터 객체</returns>
        public static T Get<T>(string uniqueKey) where T : IPreferenceObject, new()
        {
            return PreferenceController.GetPreferenceObject<T>(uniqueKey);
        }

        /// <summary>
        /// 파일명과 고유 키를 사용하여 데이터 객체를 가져옵니다.
        /// </summary>
        /// <typeparam name="T">데이터 객체 타입</typeparam>
        /// <param name="fileName">파일명</param>
        /// <param name="uniqueKey">고유 키</param>
        /// <returns>데이터 객체</returns>
        public static T Get<T>(string fileName, string uniqueKey) where T : IPreferenceObject, new()
        {
            return PreferenceController.GetPreferenceObject<T>(fileName, uniqueKey);
        }

        #endregion

        #region Save & Load

        /// <summary>
        /// 특정 파일의 데이터를 저장해야 함을 표기합니다.
        /// </summary>
        /// <param name="fileName">파일명</param>
        public static void MarkForSave(string fileName)
        {
            PreferenceController.MarkAsSaveIsRequired(fileName);
        }
        
        /// <summary>
        /// 특정 파일의 데이터를 저장합니다.
        /// </summary>
        /// <param name="fileName">파일명</param>
        /// <param name="forceSave">강제 저장 여부</param>
        public static void Save(string fileName, bool forceSave = false)
        {
            PreferenceController.Save(fileName, forceSave);
        }

        /// <summary>
        /// 특정 파일의 데이터를 로드합니다.
        /// </summary>
        /// <param name="fileName">파일명</param>
        public static void Load(string fileName)
        {
            PreferenceController.Load(fileName);
        }

        /// <summary>
        /// 모든 데이터 파일을 저장합니다.
        /// </summary>
        /// <param name="forceSave">강제 저장 여부</param>
        public static void SaveAll(bool forceSave = false)
        {
            PreferenceController.SaveAll(forceSave);
        }
        
        /// <summary>
        /// 모든 데이터 파일을 로드합니다.
        /// </summary>
        public static void LoadAll()
        {
            PreferenceController.LoadAll();
        }

        #endregion
    }
}