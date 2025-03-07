
using UnityEngine;

namespace ProjectCore.Module
{
    [ProjectModule("Device Setting", true, 900)]
    public sealed class MDeviceSO : BaseProjectModuleSO
    {
        #region Fields & Properties

        public override string Name => "Device";

        private enum TargetFrameRate
        {
            Rate30 = 30,
            Rate60 = 60,
            Rate90 = 90,
            Rate120 = 120
        }

        [Header("Screen Configure")] 
        [SerializeField] private bool _screenNeverSleep = true;
        [SerializeField] private int _vSyncCount;
        
        [Header("Target Frame Configure")]
        [SerializeField] private TargetFrameRate _targetFrameRate = TargetFrameRate.Rate60;

        [Header("Others")]
        [SerializeField] private bool _multiTouchEnabled;
        
        #endregion
        
        public override void ConfigureInitialize()
        {
            Screen.sleepTimeout = _screenNeverSleep 
                ? SleepTimeout.NeverSleep 
                : SleepTimeout.SystemSetting;
            
            QualitySettings.vSyncCount = _vSyncCount;
            
            Application.targetFrameRate = (int)_targetFrameRate;
            
            Input.multiTouchEnabled = _multiTouchEnabled;
        }
    }
}

