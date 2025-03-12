
using UnityEngine;

namespace ProjectCore.Module
{
    public static class Haptic
    {
        private const string HapticKey = "UserHaptic";
        
        public static bool Initialized { get; private set; }

        private static bool _using;

        public static bool Using
        {
            get
            {
                if (!Initialized)
                {
                    _using = PlayerPrefs.GetInt(HapticKey, 1) == 1;
                }

                return _using;
            }
            set
            {
                _using = value;
                
                PlayerPrefs.SetInt(HapticKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static void Initialize()
        {
#if UNITY_ANDROID && PROJECT_INSTALLED_HAPTIC || UNITY_IOS && PROJECT_INSTALLED_HAPTIC
            Vibration.Init();
            _using = PlayerPrefs.GetInt(HapticKey, 1) == 1;
            Initialized = true;
#endif
        }

        public static void Weak()
        {
            if (!Using)
            {
                return;
            }
            
#if UNITY_ANDROID && PROJECT_INSTALLED_HAPTIC
            Vibration.VibrateAndroid(20);
#elif UNITY_IOS && PROJECT_INSTALLED_HAPTIC
            Vibration.VibrateIOS(ImpactFeedbackStyle.Light);
#endif
        }
        
        public static void Soft()
        {
            if (!Using)
            {
                return;
            }
            
#if UNITY_ANDROID && PROJECT_INSTALLED_HAPTIC
            Vibration.VibrateAndroid(30);
#elif UNITY_IOS && PROJECT_INSTALLED_HAPTIC
            Vibration.VibrateIOS(ImpactFeedbackStyle.Soft);
#endif
        }
        
        public static void Medium()
        {
            if (!Using)
            {
                return;
            }
            
#if UNITY_ANDROID && PROJECT_INSTALLED_HAPTIC
            Vibration.VibrateAndroid(60);
#elif UNITY_IOS && PROJECT_INSTALLED_HAPTIC
            Vibration.VibrateIOS(ImpactFeedbackStyle.Medium);
#endif
        }
        
        public static void Hard()
        {
            if (!Using)
            {
                return;
            }
            
#if UNITY_ANDROID && PROJECT_INSTALLED_HAPTIC
            Vibration.VibrateAndroid(100);
#elif UNITY_IOS && PROJECT_INSTALLED_HAPTIC
            Vibration.VibrateIOS(ImpactFeedbackStyle.Heavy);
#endif
        }
    }
}