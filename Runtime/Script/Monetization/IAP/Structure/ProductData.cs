
using UnityEngine.Purchasing;

namespace ProjectCore.Monetize
{
    public class ProductData
    {
        #region Fields

        public ProductType ProductType { get; }
        public Product Product { get; }
        
        public bool IsPurchased { get; }
        public bool IsSubscribed { get; }
        
        public decimal Price { get; }
        public string ISOCurrencyCode { get; }

        #endregion

        public ProductData()
        {
            Price = 0.00m;
            ISOCurrencyCode = "USD";
            IsPurchased = false;
            IsSubscribed = false;
        }
        
        public ProductData(ProductType productType)
        {
            ProductType = productType;
            Price = 0.00m;
            ISOCurrencyCode = "USD";
            IsPurchased = false;
            IsSubscribed = false;
        }
        
        public ProductData(Product product)
        {
            Product = product;
            ProductType = product.definition.type;
            IsPurchased = product.hasReceipt;
            Price = product.metadata.localizedPrice;
            ISOCurrencyCode = product.metadata.isoCurrencyCode;
        }
        
        public string GetLocalPrice()
        {
            return $"{Price}";
        }
    }
}