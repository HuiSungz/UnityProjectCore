
using ProjectCore.Module;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ProjectCore
{
    public sealed class GlobalLifetimeScope : LifetimeScope
    {
        #region Fields

        [Header("Global Modular")] 
        [SerializeField] private ProjectModules _projectModules;
        
        private IContainerBuilder _cachedBuilder;

        #endregion
        
        protected override void Configure(IContainerBuilder builder)
        {
            gameObject.name = "[GLOBAL LIFETIME SCOPE]";
            _cachedBuilder = builder;
        }

        private void SetupProject()
        {
            _cachedBuilder.RegisterInstance(_projectModules);
            _projectModules.ConfigureInitialize();

            var injectModule = _projectModules.GetModule<MGlobalInjectableSO>();
            if (!injectModule)
            {
                return;
            }

            injectModule.ConfigureRegister(_cachedBuilder);
        }
    }
}
