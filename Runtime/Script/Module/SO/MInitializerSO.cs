
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

                LoadInitializerPrefab();
                if (!_initializeScope)
                {
                    return;
                }

                var prefabScope = Instantiate(_initializeScope);
                prefabScope.name = Name;
            }
        }
        
#if UNITY_EDITOR
        private const string RESOURCE_PATH = "Initializer";
        
        private void OnValidate()
        {
            if (!_initializeScope)
            {
                LoadInitializerPrefab();
            }
        }

        private void LoadInitializerPrefab()
        {
            var prefab = Resources.Load<ProjectInitializeLifetimeScope>(RESOURCE_PATH);
            if (prefab)
            {
                _initializeScope = prefab;

                EditorUtility.SetDirty(this);
            }
            else
            {
                Debug.LogError($"Failed to find Initializer prefab in Resources folder at path: {RESOURCE_PATH}");
            }
        }
#endif
    }
}