
using UnityEngine;

namespace ProjectCore.Monetize
{
    public abstract class CustomInterstitialConditionSO : ScriptableObject
    {
        public abstract bool IsMatched();
    }
}