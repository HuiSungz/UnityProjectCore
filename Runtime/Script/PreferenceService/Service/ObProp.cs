
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ProjectCore.Preference
{
    [Serializable]
    public class ObProp<T> where T : struct
    {
        #region Fields & Events

        // 원본 데이터 값 (실제 저장 값)
        [SerializeField] private T _value;
        
        // 시각적 데이터 값 (실제 데이터 값이 아닌 시각적으로 보여지는 값)
        [NonSerialized] [JsonIgnore] private T _visualValue;
        // 임시 데이터 값
        //  - 최대값이 설정되어 있을 경우 사용,
        //  - 최대값을 벗어나는 순간 밸류를 대체하게됨.
        //  - 저장 되지 않음
        [NonSerialized] [JsonIgnore] private T _tempValue;
        [NonSerialized] [JsonIgnore] private T _maxValue;
        // 업데이트 중인지 여부
        [NonSerialized] [JsonIgnore] private bool _isSuspended;
        // 업데이트가 중단된 경우 임시로 저장하는 큐
        [NonSerialized] [JsonIgnore] private Queue<T> _suspendedUpdates;
        // 부모 객체 (저장을 해야하는지 여부를 자동으로 알림)
        [NonSerialized] [JsonIgnore] private PreferenceObjectBase _parent;
        
        public event Action<T> OnValueChanged;
        public event Action<T> OnValueDecreased;
        public event Action<T> OnValueIncreased;
        public event Action<T> OnVisualValueChanged; 
        
        private ObProp(T initialValue)
        {
            _value = initialValue;
            _visualValue = initialValue;
            _tempValue = default;
            _maxValue = default;
            _suspendedUpdates = null;
        }
        
        public ObProp(T initialValue, PreferenceObjectBase parent) : this(initialValue)
        {
            _parent = parent;
        }
        
        [JsonProperty("Value")]
        private T SerializedSavedValue
        {
            get => _value;
            set 
            {
                _value = value;
                _visualValue = value;
            }
        }

        #endregion

        #region Properties

        [JsonIgnore]
        public T Value
        {
            get => !EqualityComparer<T>.Default.Equals(_tempValue, default) ? _tempValue : _value;
            set
            {
                var previousValue = Value;

                if (!EqualityComparer<T>.Default.Equals(_maxValue, default))
                {
                    _tempValue = value;
                }
                else
                {
                    _value = value;
                    _tempValue = default;
                }
                
                var currentValue = Value;

                if (_isSuspended)
                {
                    _suspendedUpdates ??= new Queue<T>();
                    _suspendedUpdates.Enqueue(currentValue);
                    return;
                }

                NotifyValueChanged(previousValue, currentValue);
            }
        }
        
        [JsonIgnore]
        public T VisualValue
        {
            get => !EqualityComparer<T>.Default.Equals(_visualValue, default) ? _visualValue : Value;
            set
            {
                var previousValue = _visualValue;
                _visualValue = value;
                
                if (!EqualityComparer<T>.Default.Equals(previousValue, _visualValue))
                {
                    OnVisualValueChanged?.Invoke(_visualValue);
                }
            }
        }

        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 시각적 데이터 값을 실제 데이터 값으로 동기화합니다. (이벤트 발생)
        /// </summary>
        public void VisualValueSync()
        {
            _visualValue = Value;
            OnVisualValueChanged?.Invoke(_visualValue);
        }
        
        /// <summary>
        /// 최대값을 설정합니다.
        /// </summary>
        public void SetMaxValue(T maxValue)
        {
            _maxValue = maxValue;
        }

        /// <summary>
        /// 변경 이벤트를 일시 중지합니다.
        /// </summary>
        /// <returns></returns>
        public IDisposable SuspendNotifications()
        {
            return new NotificationSuspension(this);
        }

        /// <summary>
        /// 일괄 업데이트를 수행합니다.
        /// </summary>
        public void BatchUpdate(Action<ObProp<T>> updateAction)
        {
            using (SuspendNotifications())
            {
                updateAction(this);
            }
        }

        #endregion
        
        #region Private Methods

        private void NotifyValueChanged(T previousValue, T currentValue)
        {
            if (EqualityComparer<T>.Default.Equals(previousValue, currentValue))
            {
                return;
            }

            switch (Comparer<T>.Default.Compare(currentValue, previousValue))
            {
                case < 0:
                    OnValueDecreased?.Invoke(currentValue);
                    break;
                case > 0:
                    OnValueIncreased?.Invoke(currentValue);
                    break;
            }
            
            OnValueChanged?.Invoke(currentValue);
            _parent?.OnChanged();
        }

        #endregion

        #region Nested Types

        private class NotificationSuspension : IDisposable
        {
            private readonly ObProp<T> _property;

            public NotificationSuspension(ObProp<T> property)
            {
                _property = property;
                _property._isSuspended = true;
            }

            public void Dispose()
            {
                _property._isSuspended = false;
                
                if (_property._suspendedUpdates == null || _property._suspendedUpdates.Count == 0)
                {
                    return;
                }

                var lastValue = _property._suspendedUpdates.Dequeue();
                while (_property._suspendedUpdates.Count > 0)
                {
                    lastValue = _property._suspendedUpdates.Dequeue();
                }

                _property.NotifyValueChanged(_property.Value, lastValue);
                _property._suspendedUpdates.Clear();
            }
        }

        #endregion
    }
}