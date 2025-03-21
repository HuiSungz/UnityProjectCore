
using System;
using ProjectCore.Utilities;
using UnityEngine.Purchasing;

namespace ProjectCore.Monetize
{
    public static class IAPCallback
    {
        #region Fields

        public static event Action OnInitializeComplete;
        public static event Action<string> OnPurchaseCompleted;
        public static event Action<PurchaseEventArgs> OnPurchaseCompletedWithArgs;
        public static event Action<string, PurchaseFailureReason> OnPurchaseFailed; 
        public static event Action OnRestoreCompleted;
        public static event Action OnRestoreFailed;
        
        public static event Action<IAPLoadingType> OnShowLoading;
        public static event Action<IAPLoadingType, bool> OnHideLoading;

        #endregion

        internal static void RaiseModuleInitialized()
        {
            IAP.SetupInitialize();
            
            OnInitializeComplete?.Invoke();
            
            Verbose.D("[IAP] Module initialized.");
        }
        
        internal static void RaisePurchaseCompleted(string sku)
        {
            OnPurchaseCompleted?.Invoke(sku);
        }

        internal static void RaisePurchaseCompletedWithArgs(PurchaseEventArgs args)
        {
            OnPurchaseCompletedWithArgs?.Invoke(args);
        }
        
        internal static void RaisePurchaseFailed(string sku, PurchaseFailureReason reason)
        {
            OnPurchaseFailed?.Invoke(sku, reason);
        }
        
        internal static void RaiseRestoreCompleted()
        {
            OnRestoreCompleted?.Invoke();
        }
        
        internal static void RaiseRestoreFailed()
        {
            OnRestoreFailed?.Invoke();
        }

        internal static void RaiseShowLoading(IAPLoadingType loadingType)
        {
            try
            {
                OnShowLoading?.Invoke(loadingType);
            }
            catch (Exception exception)
            {
                Verbose.E($"[IAPCallback] Error in OnShowLoading event: {exception}");
            }
        }
        
        internal static void RaiseHideLoading(IAPLoadingType loadingType, bool isCompleted)
        {
            try
            {
                ADS.AppOpenAvailable = true;
                OnHideLoading?.Invoke(loadingType, isCompleted);
            }
            catch (Exception exception)
            {
                Verbose.E($"[IAPCallback] Error in OnHideLoading event: {exception}");
            }
        }
    }
}

