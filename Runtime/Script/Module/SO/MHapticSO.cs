
namespace ProjectCore.Module
{
    [ProjectModule("Haptic(Vibration)")]
    public sealed class MHapticSO : BaseProjectModuleSO
    {
        public override string Name => "Haptic";

        public override void ConfigureInitialize()
        {
            Haptic.Initialize();
        }
    }
}