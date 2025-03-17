
using System;
using Cysharp.Threading.Tasks;
using NetworkConnector;
using ProjectCore.Utilities;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing;

namespace ProjectCore.Monetize
{
    internal partial class IAPStore
    {
        #region Fields

        private const string EnvironmentDefaultName = "production";

        #endregion

        public async void Initialize(IAPSettingSO iapSetting)
        {
            try
            {
                var isConnected = true;
                if (Monetization.Settings.UseNetworkValidate)
                {
                    isConnected = await NConnector.ValidationConnectionAsync(NetworkValidationType.All);
                }

                if (!isConnected)
                {
                    return;
                }
                
                var initializationOptions = new InitializationOptions().SetEnvironmentName(EnvironmentDefaultName);
                await UnityServices.InitializeAsync(initializationOptions).AsUniTask();

                var purchasingModule = StandardPurchasingModule.Instance();
                var configureBuilder = ConfigurationBuilder.Instance(purchasingModule);

                var iapItems = iapSetting.IAPCatalog;
                foreach (var iapItem in iapItems)
                {
                    if (!string.IsNullOrEmpty(iapItem.ID))
                    {
                        configureBuilder.AddProduct(iapItem.ID, iapItem.ProductType);
                        Verbose.D($"[IAPStore] 상품 등록: {iapItem.ID}, 타입: {iapItem.ProductType}");
                    }
                    else
                    {
                        Verbose.E("[IAPStore] 상품 ID가 비어있습니다.");
                    }
                }
                
                UnityPurchasing.Initialize(this, configureBuilder);
            }
            catch (Exception exception)
            {
                Verbose.Ex("IAPStore Initialize Error, ", exception);
            }
        }
        
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _store = controller;
            _extension = extensions;
            
            IAPCallback.RaiseModuleInitialized();
        }
        
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Verbose.E("IAPStore Initialize Failed, " + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Verbose.E("IAPStore Initialize Failed, " + error + ", " + message);
        }
    }
}