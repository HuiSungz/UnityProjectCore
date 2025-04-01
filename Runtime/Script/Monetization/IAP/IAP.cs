
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        private static Dictionary<string, IAPCatalog> _skuToCatalogMap;
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
            
            _skuToCatalogMap = new Dictionary<string, IAPCatalog>();
            var iapCatalog = _iapSetting.IAPCatalog;
            if (!(iapCatalog == null || iapCatalog.Length == 0))
            {
                foreach(var catalog in iapCatalog)
                {
                    if (!_skuToCatalogMap.TryAdd(catalog.Sku, catalog))
                    {
                        Verbose.E($"[IAP] Product with the SKU {catalog.Sku} has duplicates in the list!");
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
            
            _skuToCatalogMap = new Dictionary<string, IAPCatalog>();
            
            var iapCatalog = _iapSetting.IAPCatalog;
            if (!(iapCatalog == null || iapCatalog.Length == 0))
            {
                foreach(var catalog in iapCatalog)
                {
                    if (!_skuToCatalogMap.TryAdd(catalog.Sku, catalog))
                    {
                        Verbose.E($"[IAP] Product with the SKU {catalog.Sku} has duplicates in the list!");
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
            return _skuToCatalogMap.Values.FirstOrDefault(catalog => catalog.ID == productID);
        }

        public static IAPCatalog GetCatalogWithSku(string skuID)
        {
            return _skuToCatalogMap.GetValueOrDefault(skuID);
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
                Verbose.W($"[IAP] Product not found. SKU : {skuID}");
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

        public static string GetProductLocalPriceString(string skuID, bool isNoCurrencyCode = true)
        {
            var product = GetProductData(skuID);
            if (product != null)
            {
                var priceString = product.GetLocalPrice();
                if (isNoCurrencyCode)
                {
                    priceString = RemoveCurrencySymbols(priceString);
                }
                
                return priceString;
            }
            
            Verbose.W($"[IAP] Product not found. SKU : {skuID}");
            return string.Empty;
        }

        private static string RemoveCurrencySymbols(string priceString)
        {
            if (string.IsNullOrEmpty(priceString))
            {
                return priceString;
            }
            
            string[] currencySymbols = { "$", "₩", "€", "£", "¥", "₹", "₽", "₺", "₴", "₿", "¢" };
            foreach (var symbol in currencySymbols)
            {
                priceString = priceString.Replace(symbol, "");
            }
            
            // 추가적으로 유니코드 통화 기호 패턴을 제거합니다
            // \p{Sc}는 유니코드에서 통화 기호 범주를 나타냅니다
            priceString = Regex.Replace(priceString, @"\p{Sc}", "");
            return priceString.Trim();
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
                Verbose.W($"[IAP] Product not found. SKU : {skuID}");
                return false;
            }

            if (_iapStore.PurchaseValidation.TryGetValue(catalog.ID, out var validate))
            {
                return validate;
            }
            
            Verbose.W($"[IAP] Product not found. SKU : {skuID}");
            return false;

        }

        #endregion
    }
}
