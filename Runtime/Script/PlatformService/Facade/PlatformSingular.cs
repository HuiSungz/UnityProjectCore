
#if SDK_INSTALLED_SINGULAR
using System;

namespace ProjectCore.PlatformService
{
    public static class PlatformSingular
    {
        public static bool Initialized => PlatformSingularSO.Ref.Initialized;
        
        public static void Initialize(Action onInitializeComplete = null)
        {
            PlatformSingularSO.Ref.Initialize(onInitializeComplete);
        }
    }
}
#endif