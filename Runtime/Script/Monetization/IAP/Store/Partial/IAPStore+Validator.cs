
using System;
using System.Collections.Generic;
using ProjectCore.Utilities;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace ProjectCore.Monetize
{
    internal partial class IAPStore
    {
        #region Fields

        internal Dictionary<string, bool> PurchaseValidation = new();

        #endregion
        
        internal void ValidateAllPurchase(byte[] googleTangle, byte[] appleTangle)
        {
            foreach (var product in Store.products.all)
            {
                if (!product.hasReceipt || product.definition.type == ProductType.Consumable)
                {
                    continue;
                }
                
                var isValid = ValidateReceipt(product.receipt, product.definition.id, googleTangle, appleTangle);
                PurchaseValidation[product.definition.id] = isValid;
            }
        }
        
        internal bool ValidateReceipt(string receipt, string productID, byte[] googleTangle, byte[] appleTangle)
        {
            try
            {
                var validator = new CrossPlatformValidator(googleTangle, appleTangle, Application.identifier);
                var result = validator.Validate(receipt);

#if UNITY_ANDROID
                GooglePlayReceipt latestGoogleReceipt = null;
                foreach (var purchaseReceipt in result)
                {
                    if (purchaseReceipt is not GooglePlayReceipt googlePlayReceipt
                        || googlePlayReceipt.productID != productID)
                    {
                        continue;
                    }
                    
                    if (latestGoogleReceipt == null || googlePlayReceipt.purchaseDate > latestGoogleReceipt.purchaseDate)
                    {
                        latestGoogleReceipt = googlePlayReceipt;
                    }
                }

                return latestGoogleReceipt is { purchaseState: GooglePurchaseState.Purchased };
#elif UNITY_IOS
                AppleInAppPurchaseReceipt latestAppleReceipt = null;
                foreach (var purchaseReceipt in result)
                {
                    if (purchaseReceipt is not AppleInAppPurchaseReceipt appleReceipt
                        || appleReceipt.productID != productID)
                    {
                        continue;
                    }

                    if (latestAppleReceipt == null || appleReceipt.purchaseDate > latestAppleReceipt.purchaseDate)
                    {
                        latestAppleReceipt = appleReceipt;
                    }
                }
                
                return latestAppleReceipt != null && latestAppleReceipt.cancellationDate == DateTime.MinValue;
#endif
            }
            catch (Exception exception)
            {
                Verbose.Ex("Receipt validation failed: ", exception);
                return false;
            }
        }
    }
}