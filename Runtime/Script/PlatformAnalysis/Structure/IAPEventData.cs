
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace ProjectCore.PlatformAnalysis
{
    internal class IAPEventData
    {
        public string StoreKey { get; }
        public Product Product { get; }
        public string Revenue { get; }
        public string TransactionId { get; }
        public string ProductId { get; }
        public Dictionary<string, object> Attributes { get; }
        
        private IAPEventData(Builder builder)
        {
            StoreKey = builder.StoreKey;
            Product = builder.Product;
            Revenue = builder.Revenue;
            TransactionId = Product?.transactionID;
            ProductId = Product?.definition.id;
            Attributes = builder.Attributes ?? new Dictionary<string, object>();
        }
        
        public class Builder
        {
            public string StoreKey { get; private set; }
            public Product Product { get; private set; }
            public string Revenue { get; private set; }
            public Dictionary<string, object> Attributes { get; private set; }
            
            public Builder(string storeKey, Product product)
            {
                StoreKey = storeKey;
                Product = product;
                Attributes = new Dictionary<string, object>();
            }
            
            public Builder SetRevenue(string revenue)
            {
                Revenue = revenue;
                return this;
            }
            
            public Builder AddAttribute(string key, object value)
            {
                if (Attributes == null)
                {
                    Attributes = new Dictionary<string, object>();
                }
                
                Attributes[key] = value;
                return this;
            }
            
            public Builder SetAttributes(Dictionary<string, object> attributes)
            {
                Attributes = attributes;
                return this;
            }
            
            public IAPEventData Build()
            {
                return new IAPEventData(this);
            }
        }
    }
}