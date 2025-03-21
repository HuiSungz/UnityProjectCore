
using System.Collections.Generic;
using System.Linq;
using ProjectCore.Utilities;
using UnityEngine;
using UnityEngine.Purchasing;

namespace ProjectCore.Monetize
{
    public static class IAP
    {
        #region Fields

        private static bool _isInitialized;
        public static bool IsInitialized => _isInitialized;

        private static Dictionary<string, IAPCatalog> _typeToCatalogMap;
        private static IAPStore _iapStore;
        private static IAPSettingSO _iapSetting;

        #endregion

        public static void Configure(MonetizationSettingSO monetizationSettings)
        {
            if (_isInitialized)
            {
                Debug.LogError("[IAP] Already initialized.");
                return;
            }

            _iapSetting = monetizationSettings.IAPSetting;
            if (!_iapSetting.AutoInitialize)
            {
                return;
            }
            
            _typeToCatalogMap = new Dictionary<string, IAPCatalog>();
            
            var iapCatalog = _iapSetting.IAPCatalog;
            if (!(iapCatalog == null || iapCatalog.Length == 0))
            {
                foreach(var catalog in iapCatalog)
                {
                    if (!_typeToCatalogMap.TryAdd(catalog.ID, catalog))
                    {
                        Verbose.E($"[IAP] Product with the type {catalog.ID} has duplicates in the list!");
                    }
                }
            }
            
            _iapStore = new IAPStore();
            _iapStore.Initialize(_iapSetting);
        }

        public static void ManuallyInitialize()
        {
            if (_isInitialized)
            {
                Debug.LogError("[IAP] Already initialized.");
                return;
            }
            
            _typeToCatalogMap = new Dictionary<string, IAPCatalog>();
            
            var iapCatalog = _iapSetting.IAPCatalog;
            if (!(iapCatalog == null || iapCatalog.Length == 0))
            {
                foreach(var catalog in iapCatalog)
                {
                    if (!_typeToCatalogMap.TryAdd(catalog.ID, catalog))
                    {
                        Verbose.E($"[IAP] Product with the type {catalog.ID} has duplicates in the list!");
                    }
                }
            }
            
            _iapStore = new IAPStore();
            _iapStore.Initialize(_iapSetting);
        }
        
        internal static void SetupInitialize()
        {
            _isInitialized = true;
        }

        #region Access

        public static IAPCatalog GetCatalogWithID(string productID)
        {
            return _typeToCatalogMap.Values.FirstOrDefault(catalog => catalog.ID == productID);
        }

        public static IAPCatalog GetCatalogWithSku(string skuID)
        {
            return _typeToCatalogMap.GetValueOrDefault(skuID);
        }
        
        public static Product GetProductWithSku(string skuID)
        {
            var catalog = GetCatalogWithSku(skuID);
            return catalog != null ? IAPStore.Store.products.WithID(catalog.ID) : null;
        }

        public static void RestorePurchase()
        {
            if (!Monetization.IsActivate || !_isInitialized)
            {
                return;
            }
            
            _iapStore.RestorePurchase();
        }
        
        public static void PurchaseProduct(string skuID)
        {
            if (!Monetization.IsActivate)
            {
                Verbose.W("[IAP] Monetization is not activated.");
                return;
            }
            
            if(!_isInitialized)
            {
                Verbose.W("[IAP] Module is not initialized.");
                return;
            }
            
            _iapStore.PurchaseProduct(skuID);
        }
        
        public static ProductData GetProductData(string skuID)
        {
            if(!Monetization.IsActivate || !_isInitialized)
            {
                return new ProductData();
            }

            var productData = _iapStore.GetProductData(skuID);
            if (productData == null)
            {
                Verbose.W($"[IAP] Product not found. ID : {skuID}");
            }

            return productData;
        }
        
        public static bool IsSubscription(string skuID)
        {
            if(!Monetization.IsActivate || !_isInitialized)
            {
                return false;
            }

            return _iapStore.IsSubscribed(skuID);
        }

        public static string GetProductLocalPriceString(string skuID)
        {
            var product = GetProductData(skuID);
            if (product != null)
            {
                return product.GetLocalPrice();
            }
            
            Verbose.W($"[IAP] Product not found. ID : {skuID}");
            return string.Empty;
        }

        public static void ValidateReceiptAll(byte[] googleTangle, byte[] appleTangle)
        {
            if(!Monetization.IsActivate || !_isInitialized)
            {
                Verbose.W("[IAP] Monetization is not activated or not initialized.");
                return;
            }
            
            _iapStore.ValidateAllPurchase(googleTangle, appleTangle);
        }
        
        public static bool ValidateReceipt(string skuID, byte[] googleTangle, byte[] appleTangle)
        {
            if(!Monetization.IsActivate || !_isInitialized)
            {
                Verbose.W("[IAP] Monetization is not activated or not initialized.");
                return false;
            }

            var product = GetProductData(skuID);
            if (product.ProductType == ProductType.Consumable
                || !product.Product.hasReceipt)
            {
                return false;
            }
            
            var isValid = _iapStore.ValidateReceipt(
                product.Product.receipt, product.Product.definition.id,
                googleTangle, appleTangle);
            return isValid;
        }

        public static bool IsValidatedReceipt(string skuID)
        {
            if(!Monetization.IsActivate || !_isInitialized)
            {
                Verbose.W("[IAP] Monetization is not activated or not initialized.");
                return false;
            }

            var catalog = GetCatalogWithSku(skuID);
            if (catalog == null)
            {
                Verbose.W($"[IAP] Product not found. ID : {skuID}");
                return false;
            }

            if (_iapStore.PurchaseValidation.TryGetValue(catalog.ID, out var validate))
            {
                return validate;
            }
            
            Verbose.W($"[IAP] Product not validated. ID : {skuID}");
            return false;

        }

        #endregion
    }
}
