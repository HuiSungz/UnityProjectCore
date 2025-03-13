
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Monetize
{
    public static partial class ADS
    {
        #region Fields

        private static readonly List<Action> MainThreadEvents = new();

        #endregion
        
        private static void UpdateProcess()
        {
            if (!_isConfigure)
            {
                return;
            }

            if (MainThreadEvents.Count > 0)
            {
                var tempEvents = new List<Action>(MainThreadEvents);
                MainThreadEvents.Clear();
                
                foreach(var tempEvent in tempEvents)
                {
                    tempEvent?.Invoke();
                }
            }

            if (!Setting.AutoShowInterstitial)
            {
                return;
            }

            if (!(_lastInterstitialTime < Time.time))
            {
                return;
            }

            ShowInterstitial(null);
            ResetInterstitialDelayTime();
        }
        
        internal static void CallEventInMainThread(Action callback)
        {
            if (callback != null)
            {
                MainThreadEvents.Add(callback);
            }
        }

        #region Private Class

        private class AdsEventDispatcher : MonoBehaviour
        {
            private void Awake()
            {
                DontDestroyOnLoad(gameObject);
            }
            
            private void Update()
            {
                UpdateProcess();
            }
        }

        #endregion
    }
}
