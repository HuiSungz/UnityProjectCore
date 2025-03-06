
using System;
using UnityEngine;
using Newtonsoft.Json;

namespace ProjectCore.Preference
{
    /// <summary>
    /// 사용자 설정 데이터를 담는 컨테이너 클래스입니다.
    /// JSON 직렬화를 통해 데이터의 영속성을 보장합니다.
    /// </summary>
    [Serializable]
    internal sealed class PreferenceContainer
    {
        #region Fields
        
        [SerializeField] private int _hash;
        [SerializeField] private string _json;
        [NonSerialized] private IPreferenceObject _prefObject;
        [NonSerialized] private bool _isRestored;
        
        public int Hash => _hash;
        public IPreferenceObject Object => _prefObject;
        public bool IsRestored => _isRestored;
        
        #endregion
        
        public PreferenceContainer() { }
        
        public PreferenceContainer(int hash, IPreferenceObject prefObject)
        {
            _hash = hash;
            _prefObject = prefObject;
            _isRestored = true;
            
            // 객체를 JSON으로 직렬화하여 저장
            _json = JsonConvert.SerializeObject(prefObject, new JsonSerializerSettings 
            { 
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore 
            });
        }
        
        /// <summary>
        /// 메모리 상의 객체 데이터를 JSON으로 직렬화하여 저장합니다.
        /// 
        /// 이 메서드는 다음과 같은 역할을 수행합니다:
        /// 1. 객체의 현재 상태를 영구 저장 가능한 형태로 변환
        /// 2. 객체의 모든 필드와 속성을 JSON 형식으로 캡처
        /// 3. 메모리에서 변경된 데이터를 저장 준비 상태로 만듦
        /// 
        /// 저장 작업 직전에 호출되어 최신 객체 상태를 저장합니다.
        /// </summary>
        public void Flush()
        {
            if (_prefObject != null)
            {
                _prefObject.Flush();
                _json = JsonConvert.SerializeObject(_prefObject, new JsonSerializerSettings 
                { 
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore 
                });
            }
        }
        
        /// <summary>
        /// 저장된 JSON 데이터로부터 메모리 객체를 복원합니다.
        /// 
        /// 이 메서드는 다음과 같은 역할을 수행합니다:
        /// 1. 직렬화된 JSON 데이터를 실제 객체로 변환 (역직렬화)
        /// 2. 이전에 저장된 객체의 상태를 메모리에 다시 로드
        /// 3. 게임 실행 중 객체에 접근할 때 처음 한 번만 수행 (지연 로딩)
        /// 
        /// 객체가 필요할 때만 호출되어 메모리를 효율적으로 관리합니다.
        /// </summary>
        /// <typeparam name="T">복원할 객체 타입</typeparam>
        public void Restore<T>() where T : IPreferenceObject, new()
        {
            if (_isRestored)
            {
                return;
            }
            
            try
            {
                _prefObject = !string.IsNullOrEmpty(_json) 
                    ? JsonConvert.DeserializeObject<T>(_json) 
                    : new T();
                _isRestored = true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[PreferenceContainer] 객체 JSON 역직렬화 실패: {exception.Message}");
                _prefObject = new T();
                _isRestored = true;
            }
        }
    }
}