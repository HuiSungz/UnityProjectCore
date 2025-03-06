
#if SDK_INSTALLED_GAMEANALYTICS
using System;

namespace ProjectCore.Module
{
    public static class PlatformGameAnalytics
    {
        public static bool Initialized => PlatformGameAnalyticsSO.Ref.Initialized;
        
        public static void Initialize(Action onInitializeComplete = null)
        {
            PlatformGameAnalyticsSO.Ref.Initialize(onInitializeComplete);
        }
    }
}
#endif