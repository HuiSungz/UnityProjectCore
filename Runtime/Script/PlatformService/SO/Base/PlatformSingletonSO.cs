
using UnityEngine;

namespace ProjectCore.PlatformService
{
    public abstract class PlatformSingletonSO<T> : PlatformServiceSO where T : PlatformSingletonSO<T>
    {
        #region Singleton

        private static readonly object Lock = new();
        private static volatile T _sInstance;

        protected bool IsInitialized;
        public virtual bool Initialized => IsInitialized;

        public static T Ref
        {
            get
            {
                lock (Lock)
                {
                    if (_sInstance)
                    {
                        return _sInstance;
                    }

                    Debug.LogError("PlatformSingleton reference is null");
                    return null;
                }
            }
        }

        #endregion

        protected void OnEnable()
        {
            lock (Lock)
            {
#if UNITY_EDITOR
                IsInitialized = false;
#endif
                _sInstance = this as T;
            }
        }
    }
}