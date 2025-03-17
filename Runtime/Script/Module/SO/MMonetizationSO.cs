
using ProjectCore.Monetize;
using UnityEngine;

namespace ProjectCore.Module
{
    [ProjectModule("Monetization")]
    public class MMonetizationSO : BaseProjectModuleSO
    {
        [SerializeField] private MonetizationSettingSO _settings;
        public override string Name => "Monetization";
        public override void ConfigureInitialize()
        {
            Monetization.Configure(_settings);
            ADS.Configure(_settings);
            IAP.Configure(_settings);
        }
    }
}