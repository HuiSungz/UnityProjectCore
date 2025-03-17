
using ProjectCore.PlatformAnalysis;
using UnityEngine;

namespace ProjectCore.Module
{
    [ProjectModule("Analysis Event")]
    public sealed class MAnalysisSO : BaseProjectModuleSO
    {
        #region Fields

        [SerializeField] private bool _autoSendADSRevenue = true;
        [SerializeField] private bool _autoSendIAPRevenue = true;

        #endregion
        
        public override string Name => "Analysis Event";
        public override void ConfigureInitialize()
        {
            var newSetting = new AnalysisSettings(_autoSendADSRevenue, _autoSendIAPRevenue);
            Analysis.Initialize(newSetting);
        }
    }
}