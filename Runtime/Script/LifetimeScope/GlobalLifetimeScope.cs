
using ProjectCore.Module;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ProjectCore
{
    public sealed class GlobalLifetimeScope : LifetimeScope
    {
        #region Fields

        private ProjectModules _projectModules;
        private IContainerBuilder _cachedBuilder;

        #endregion
        
        protected override void Configure(IContainerBuilder builder)
        {
            gameObject.name = "[GLOBAL LIFETIME SCOPE]";
            _cachedBuilder = builder;

            LoadProjectModules();
            LoadInjectModules();
        }

        private void Start()
        {
            _projectModules.ConfigureInitialize();
        }

        private void LoadProjectModules()
        {
            _projectModules = Resources.Load<ProjectModules>("Project Modules");
            
            if (!_projectModules)
            {
                Debug.LogError("Project Modules 에셋을 Resources 폴더에서 찾을 수 없습니다. " +
                               "패키지나 어셈블리 내의 Resources 폴더도 확인하세요.");
            }
            
            _cachedBuilder.RegisterInstance(_projectModules);
        }

        private void LoadInjectModules()
        {
            var injectModule = _projectModules.GetModule<MGlobalInjectableSO>();
            if (!injectModule)
            {
                return;
            }

            injectModule.ConfigureRegister(_cachedBuilder);
        }
    }
}
