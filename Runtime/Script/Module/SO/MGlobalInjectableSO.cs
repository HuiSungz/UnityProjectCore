
using UnityEngine;
using VContainer;

namespace ProjectCore.Module
{
    [ProjectModule("Global Injectable (VContainer)")]
    public sealed class MGlobalInjectableSO : BaseProjectModuleSO
    {
        [SerializeField] private BaseInjectableSO[] _injectables;
        
        public override string Name => "Global Injectable";
        public override void ConfigureInitialize() { }
        
        public void ConfigureRegister(IContainerBuilder globalBuilder)
        {
            foreach(var injectable in _injectables)
            {
                injectable.ConfigureRegister(globalBuilder);
            }
        }
    }
}
