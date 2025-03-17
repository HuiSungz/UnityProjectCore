
using ProjectCore.Utilities;
using UnityEngine.Purchasing;

namespace ProjectCore.Monetize
{
    internal partial class IAPStore : IDetailedStoreListener
    {
        #region Fields

        private static IStoreController _store;
        private static IExtensionProvider _extension;
        
        public static IStoreController Store => _store;
        public static IExtensionProvider Extension => _extension;

        #endregion

        #region Manually

        public void PurchaseProduct(string skuID)
        {
            if (!IAP.IsInitialized)
            {
                Verbose.E("[IAP] Not initialized.");
                return;
            }
            
            IAPCallback.RaiseShowLoading(IAPLoadingType.Purchase);
            
            var catalog = IAP.GetCatalogWithSku(skuID);
            if (catalog != null)
            {
                _store.InitiatePurchase(catalog.ID);
            }
        }

        public void RestorePurchase()
        {
            if (!IAP.IsInitialized)
            {
                Verbose.E("[IAP] Not initialized.");
                return;
            }

            IAPCallback.RaiseShowLoading(IAPLoadingType.Restore);
            
            var appleExtensions = _extension.GetExtension<IAppleExtensions>();
            appleExtensions.RestoreTransactions((result, message) =>
            {
                if (result)
                {
                    IAPCallback.RaiseRestoreCompleted();
                }
                else
                {
                    IAPCallback.RaiseRestoreFailed();
                }

                IAPCallback.RaiseHideLoading(IAPLoadingType.Restore, result);
            });
        }
        
        public ProductData GetProductData(string skuID)
        {
            if (!IAP.IsInitialized)
            {
                Verbose.E("[IAP] Not initialized.");
                return null;
            }

            var catalog = IAP.GetCatalogWithSku(skuID);
            return catalog != null 
                ? new ProductData(_store.products.WithID(catalog.ID)) 
                : null;
        }

        public bool IsSubscribed(string skuID)
        {
            var catalog = IAP.GetCatalogWithSku(skuID);
            if (catalog == null)
            {
                return false;
            }
            
            var product = _store.products.WithID(catalog.ID);

            if (product?.receipt == null)
            {
                return false;
            }
            
            var subscriptionManager = new SubscriptionManager(product, null);
            var subscriptionInfo = subscriptionManager.getSubscriptionInfo();
            return subscriptionInfo.isSubscribed() == Result.True;
        }

        #endregion
    }
}