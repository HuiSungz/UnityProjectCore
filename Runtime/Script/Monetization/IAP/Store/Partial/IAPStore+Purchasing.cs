
using ProjectCore.Utilities;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace ProjectCore.Monetize
{
    internal partial class IAPStore
    {
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var catalog = IAP.GetCatalogWithID(purchaseEvent.purchasedProduct.definition.id);
            if (catalog != null)
            {
                IAPCallback.RaisePurchaseCompleted(catalog.Sku);
                IAPCallback.RaisePurchaseCompletedWithArgs(purchaseEvent);
            }
            else
            {
                Verbose.W($"[IAP] Product with the type {purchaseEvent.purchasedProduct.definition.id} can't be found!");
            }
            
            IAPCallback.RaiseHideLoading(IAPLoadingType.Purchase, true);

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Verbose.D("Purchase failed: " + product.definition.id + " Reason: " + failureReason);
            
            var catalog = IAP.GetCatalogWithID(product.definition.id);
            if (catalog != null)
            {
                IAPCallback.RaisePurchaseFailed(catalog.Sku, failureReason);
            }
            else
            {
                Verbose.W($"[IAP] Product with the type {product.definition.id} can't be found!");
            }
            
            IAPCallback.RaiseHideLoading(IAPLoadingType.Purchase,false);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Verbose.D("Purchase failed: " + product.definition.id + " Reason: " + failureDescription.reason);
            
            var catalog = IAP.GetCatalogWithID(product.definition.id);
            if (catalog != null)
            {
                IAPCallback.RaisePurchaseFailed(catalog.Sku, failureDescription.reason);
            }
            else
            {
                Verbose.W($"[IAP] Product with the type {product.definition.id} can't be found!");
            }
            
            IAPCallback.RaiseHideLoading(IAPLoadingType.Purchase,false);
        }
    }
}