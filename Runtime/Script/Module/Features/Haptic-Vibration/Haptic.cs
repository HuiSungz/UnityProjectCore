
namespace ProjectCore.Module
{
    public static class Haptic
    {
        public static bool Initialized { get; private set; }
        public static bool Using { get; set; } = true;

        public static void Initialize()
        {
#if UNITY_ANDROID || UNITY_IOS
            Vibration.Init();
            Initialized = true;
#endif
        }

        public static void Weak()
        {
            if (!Using)
            {
                return;
            }
            
#if UNITY_ANDROID
            Vibration.VibrateAndroid(20);
#elif UNITY_IOS
            Vibration.VibrateIOS(ImpactFeedbackStyle.Light);
#endif
        }
        
        public static void Soft()
        {
            if (!Using)
            {
                return;
            }
            
#if UNITY_ANDROID
            Vibration.VibrateAndroid(30);
#elif UNITY_IOS
            Vibration.VibrateIOS(ImpactFeedbackStyle.Soft);
#endif
        }
        
        public static void Medium()
        {
            if (!Using)
            {
                return;
            }
            
#if UNITY_ANDROID
            Vibration.VibrateAndroid(60);
#elif UNITY_IOS
            Vibration.VibrateIOS(ImpactFeedbackStyle.Medium);
#endif
        }
        
        public static void Hard()
        {
            if (!Using)
            {
                return;
            }
            
#if UNITY_ANDROID
            Vibration.VibrateAndroid(100);
#elif UNITY_IOS
            Vibration.VibrateIOS(ImpactFeedbackStyle.Heavy);
#endif
        }
    }
}