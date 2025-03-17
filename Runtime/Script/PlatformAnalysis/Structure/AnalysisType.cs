
using System;

namespace ProjectCore.PlatformAnalysis
{
    [Flags]
    public enum AnalysisType
    {
        Unknown = 0,
        Singular = 1 << 0,
        GameAnalytics = 1 << 1,
        Firebase = 1 << 2,
        
        All = Singular | GameAnalytics | Firebase,
        SG = Singular | GameAnalytics,
        SF = Singular | Firebase,
        GF = GameAnalytics | Firebase
    }
}