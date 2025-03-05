
using UnityEngine;

namespace ProjectCore.Module
{
    public abstract class BaseLockSO : ScriptableObject
    {
        protected static readonly object Lock = new();
    }
}
