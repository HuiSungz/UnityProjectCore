
namespace ProjectCore.Monetize
{
    public static class Monetization
    {
        #region Fields

        public static bool IsActivate { get; private set; }
        public static bool UseNetworkValidate { get; private set; }
        
        public static MonetizationSettingSO Settings { get; private set; }

        #endregion
        
        public static void Configure(MonetizationSettingSO setting)
        {
            Settings = setting;
            
            UpdateProperties(setting);
        }

        private static void UpdateProperties(MonetizationSettingSO settings)
        {
            UseNetworkValidate = settings.UseNetworkValidate;
            IsActivate = settings.IsActivate;
        }
    }
}