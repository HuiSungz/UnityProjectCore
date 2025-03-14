
using ProjectCore.Attributes;
using UnityEngine;

namespace ProjectCore.Monetize
{
    public sealed class MonetizationSettingSO : ScriptableObject
    {
        [SerializeField] [ReadOnly] private AdsSettingSO _adsSetting;
        [SerializeField] [ReadOnly] private IAPSettingSO _iapSetting;
        
        [SerializeField] private bool _useNetworkValidate = true;
        [SerializeField] private bool _isActivate = true;
        
        public AdsSettingSO AdsSetting => _adsSetting;
        public IAPSettingSO IAPSetting => _iapSetting;
        
        public bool UseNetworkValidate => _useNetworkValidate;
        public bool IsActivate => _isActivate;
    }
}