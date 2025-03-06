
using UnityEngine;

namespace ProjectCore.Module
{
    [ProjectModule("Initializer", true, 999)]
    public sealed class MInitializerSO : BaseProjectModuleSO
    {
        public override string Name => "Initializer";

        [Header("Take the lifetime scope from the project.")] [SerializeField]
        private ProjectInitializeLifetimeScope _initializeScope;

        [Space] [SerializeField] private bool _autoInitialize = true;
        
        public override void ConfigureInitialize()
        {
            if (!_autoInitialize)
            {
                return;
            }

            if (_initializeScope)
            {
                var prefabScope = Instantiate(_initializeScope);
                prefabScope.name = Name;
            }
            else
            {
                Debug.LogError($"Initializer prefab is not set in {Name}.");
            }
        }
    }
}