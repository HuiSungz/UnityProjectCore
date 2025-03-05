
using UnityEngine;
using VContainer;

namespace ProjectCore.Module
{
    public abstract class BaseInjectableSO : ScriptableObject
    {
        public abstract void ConfigureRegister(IContainerBuilder globalBuilder);
    }
}