
#if SDK_INSTALLED_FIREBASE
using System;

namespace ProjectCore.Module
{
    public static class PlatformFirebase
    {
        public static bool Initialized => PlatformFirebaseSO.Ref.Initialized;
        
        public static void Initialize(Action onInitializeComplete = null)
        {
            PlatformFirebaseSO.Ref.Initialize(onInitializeComplete);
        }
    }
}
#endif