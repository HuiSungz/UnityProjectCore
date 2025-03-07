
using System;
using UnityEngine;

namespace ProjectCore.Base
{
    public abstract class BaseAttributeRequiredSO<T> : ScriptableObject where T : Attribute
    {
        protected virtual void OnEnable()
        {
            ValidateRequiredAttributes();
        }

        private void ValidateRequiredAttributes()
        {
            var thisType = GetType();
            var attributes = thisType.GetCustomAttributes(typeof(T), true);
            
            if (attributes.Length == 0)
            {
                Debug.LogError($"ScriptableObject {name} ({thisType.Name})에 [{typeof(T).Name}] 어트리뷰트가 없습니다!");
            }
        }
    }
}
