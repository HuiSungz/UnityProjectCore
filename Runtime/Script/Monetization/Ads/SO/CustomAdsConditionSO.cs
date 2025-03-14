
using UnityEngine;

namespace ProjectCore.Monetize
{
    public abstract class CustomAdsConditionSO : ScriptableObject
    {
        public abstract bool IsMatched();
    }
}