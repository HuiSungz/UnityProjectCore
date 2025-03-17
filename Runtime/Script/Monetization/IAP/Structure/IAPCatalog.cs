
using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace ProjectCore.Monetize
{
    [Serializable]
    public class IAPCatalog
    {
        #region Fields & Properties

        [SerializeField] private string _androidID;
        [SerializeField] private string _iosID;
        [SerializeField] private string _productSku;
        [SerializeField] private ProductType _productType;

        public string ID
        {
            get
            {
#if UNITY_ANDROID
                return _androidID;
#elif UNITY_IOS
                return _iosID;
#else
                return string.Format("unknown_platform_{0}", _productKey);
#endif
            }
        }
        
        public string Sku
        {
            get => _productSku;
            set => _productSku = value;
        }
        
        public ProductType ProductType
        {
            get => _productType;
            set => _productType = value;
        }

        #endregion
        
        public IAPCatalog() { }
        
        public IAPCatalog(string id, string sku, ProductType productType)
        {
            _androidID = id;
            _iosID = id;
            _productSku = sku;
            _productType = productType;
        }

        public IAPCatalog(string androidID, string iosID, string sku, ProductType productType)
        {
            _androidID = androidID;
            _iosID = iosID;
            _productSku = sku;
            _productType = productType;
        }
    }
}