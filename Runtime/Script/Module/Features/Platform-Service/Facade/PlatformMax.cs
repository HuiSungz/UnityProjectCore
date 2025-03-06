
#if SDK_INSTALLED_APPLOVINMAX
using System;

namespace ProjectCore.Module
{
    public static class PlatformMax
    {
        public static bool Initialized => PlatformMaxSO.Ref.Initialized;
        
        public static void Initialize(Action onInitializeComplete = null)
        {
            PlatformMaxSO.Ref.Initialize(onInitializeComplete);
        }
    }
}
#endif