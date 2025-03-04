
using UnityEngine;

public class ProjectModules : BaseDescriptionSO
{
    #region Fields

    [SerializeField] private BaseProjectModuleSO[] _projectModules;

    public BaseProjectModuleSO[] Modules => _projectModules;

    #endregion
    
    

    #region Public Methods

    public void Initialize()
    {
        foreach (var module in _projectModules)
        {
            if (!module)
            {
                continue;
            }
            
            module.ConfigureInitialize();
        }
    }

    public T GetModule<T>() where T : BaseProjectModuleSO
    {
        foreach (var module in _projectModules)
        {
            if (module && module is T convertedModule)
            {
                return convertedModule;
            }
        }

        return null;
    }

    #endregion
}
