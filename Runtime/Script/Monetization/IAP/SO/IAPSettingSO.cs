
using UnityEngine;

namespace ProjectCore.Monetize
{
    public class IAPSettingSO : ScriptableObject
    {
        [Header("Auto Individually init")]
        [SerializeField] private bool _autoInitialize = true;
        public bool AutoInitialize => _autoInitialize;
        
        [SerializeField] private IAPCatalog[] _iapCatalog;
        public IAPCatalog[] IAPCatalog => _iapCatalog;
    }
}