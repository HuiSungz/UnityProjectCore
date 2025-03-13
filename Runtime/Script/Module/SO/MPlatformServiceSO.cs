
using ProjectCore.PlatformService;
using UnityEngine;

namespace ProjectCore.Module
{
    [ProjectModule("PlatformService (SDKs)")]
    public sealed class MPlatformServiceSO : BaseProjectModuleSO
    {
        [SerializeField] private PlatformServiceSO[] _platformServices;
        public override string Name => "Platform Service";
        public override void ConfigureInitialize() { }
    }
}