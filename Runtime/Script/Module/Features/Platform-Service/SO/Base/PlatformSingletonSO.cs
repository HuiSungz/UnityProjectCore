

using UnityEngine;

namespace ProjectCore.Module
{
    public abstract class PlatformSingletonSO<T> : PlatformServiceSO where T : PlatformSingletonSO<T>
    {
        #region Singleton

        private static readonly object Lock = new();
        private static volatile T _sInstance;

        protected bool IsInitialized;

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
        
        
    }
}