
using ProjectCore.Monetize;

namespace ProjectCore.PlatformAnalysis
{
    /// <summary>
    /// 광고 수익 데이터를 표현하는 모델 클래스
    /// </summary>
    internal class AdRevenueData
    {
        public string Platform { get; }
        public string NetworkName { get; }
        public string AdUnitId { get; }
        public string PlacementName { get; }
        public string AdFormat { get; }
        public string CurrencyCode { get; }
        public double Revenue { get; }
        public string Precision { get; }
        public AdvertisementType AdType { get; }

        private AdRevenueData(Builder builder)
        {
            Platform = builder.Platform;
            NetworkName = builder.NetworkName;
            AdUnitId = builder.AdUnitId;
            PlacementName = builder.PlacementName;
            AdFormat = builder.AdFormat;
            CurrencyCode = builder.CurrencyCode;
            Revenue = builder.Revenue;
            Precision = builder.Precision;
            AdType = builder.AdType;
        }

        public class Builder
        {
            public string Platform { get; private set; }
            public string NetworkName { get; private set; }
            public string AdUnitId { get; private set; }
            public string PlacementName { get; private set; }
            public string AdFormat { get; private set; }
            public string CurrencyCode { get; private set; }
            public double Revenue { get; private set; }
            public string Precision { get; private set; }
            public AdvertisementType AdType { get; private set; }

            public Builder(string platform, double revenue)
            {
                Platform = platform;
                Revenue = revenue;
                CurrencyCode = "USD";
                NetworkName = platform;
                AdType = AdvertisementType.Unknown;
            }

            public Builder SetNetworkName(string networkName)
            {
                NetworkName = networkName;
                return this;
            }

            public Builder SetAdUnitId(string adUnitId)
            {
                AdUnitId = adUnitId;
                return this;
            }

            public Builder SetPlacementName(string placementName)
            {
                PlacementName = placementName;
                return this;
            }

            public Builder SetAdFormat(string adFormat)
            {
                AdFormat = adFormat;
                return this;
            }

            public Builder SetCurrencyCode(string currencyCode)
            {
                CurrencyCode = currencyCode;
                return this;
            }

            public Builder SetRevenue(double revenue)
            {
                Revenue = revenue;
                return this;
            }

            public Builder SetPrecision(string precision)
            {
                Precision = precision;
                return this;
            }

            public Builder SetAdType(AdvertisementType adType)
            {
                AdType = adType;
                return this;
            }

            public AdRevenueData Build()
            {
                return new AdRevenueData(this);
            }
        }
    }
}