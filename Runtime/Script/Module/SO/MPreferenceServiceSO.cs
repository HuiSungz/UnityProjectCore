
using ProjectCore.Preference;
using UnityEngine;

namespace ProjectCore.Module
{
    [ProjectModule("Preference (Save)", true, 980)]
    public class MPreferenceServiceSO : BaseProjectModuleSO
    {
        [SerializeField] private PreferenceSettingSO _setting;
        public override string Name => "Preference";
        public override void ConfigureInitialize()
        {
            Prefs.Initialize(_setting);
        }
    }
}

