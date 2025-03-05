
using UnityEngine;

namespace ProjectCore.Module
{
    [ProjectModule("Global Singleton SO")]
    public class MGlobalSingletonSO : BaseProjectModuleSO
    {
        [Header("Inherited. BaseSingletonsSO")]
        [SerializeField] private BaseLockSO[] _singletons;
        public override string Name => "Global Singletons SO";
        public override void ConfigureInitialize() { }
    }
}
