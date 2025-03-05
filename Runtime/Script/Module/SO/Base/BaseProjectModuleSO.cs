
using UnityEngine;

namespace ProjectCore.Module
{
    public abstract class BaseProjectModuleSO : ScriptableObject
    {
        public abstract string Name { get; }
        public abstract void ConfigureInitialize();
    }
}