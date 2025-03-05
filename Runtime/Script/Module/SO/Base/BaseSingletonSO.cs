
namespace ProjectCore.Module
{
    public abstract class BaseSingletonSO<T> : BaseLockSO where T : BaseSingletonSO<T>
    {
        #region Fields
        
        protected static volatile T _sInstance;

        public static T Ref
        {
            get
            {
                lock (Lock)
                {
                    return _sInstance;
                }
            }
        }

        #endregion

        private void OnEnable()
        {
            lock (Lock)
            {
                _sInstance = this as T;
            }
        }
    }
}