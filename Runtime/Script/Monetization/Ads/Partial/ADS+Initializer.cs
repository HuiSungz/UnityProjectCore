
using System;
using System.Collections.Generic;
using ProjectCore.Utilities;
using UnityEngine;

namespace ProjectCore.Monetize
{
    public static partial class ADS
    {
        #region Fields
        
        private static bool _isInitialize;
        public static bool IsInitialize => _isInitialize;

        #endregion

        public static void ManuallyInitialize()
        {
            _adProviders = GetAdProviders();
            _lastInterstitialTime = Time.time + _setting.InterstitialStartDelay;
            _advertisementProviders = new Dictionary<AdProviderType, AdProvider>();
            
            InstantiateDispatcher();

            foreach (var adProvider in _adProviders)
            {
                if (!IsProviderEnabled(adProvider.ProviderType))
                {
                    continue;
                }
                
                adProvider.LinkSettings(Monetization.Settings);
                _advertisementProviders.Add(adProvider.ProviderType, adProvider);
            }

            InitializeProviders();
        }

        private static void InitializeInternal(MonetizationSettingSO monetizationSetting)
        {
            _adProviders = GetAdProviders();
            _lastInterstitialTime = Time.time + _setting.InterstitialStartDelay;
            _advertisementProviders = new Dictionary<AdProviderType, AdProvider>();
            
            InstantiateDispatcher();

            foreach (var adProvider in _adProviders)
            {
                if (!IsProviderEnabled(adProvider.ProviderType))
                {
                    continue;
                }
                
                adProvider.LinkSettings(monetizationSetting);
                _advertisementProviders.Add(adProvider.ProviderType, adProvider);
            }

            InitializeProviders();
        }
        
        private static void InstantiateDispatcher()
        {
            Executor.Initialize();
            
            var dispatcher = new GameObject("AdsEventDispatcher")
            {
                hideFlags = HideFlags.HideInHierarchy
            };
            dispatcher.AddComponent<AdsEventDispatcher>();
        }
        
        private static async void InitializeProviders()
        {
            try
            {
                foreach (var adProvider in _advertisementProviders.Values)
                {
                    Verbose.D($"[ADS] : {adProvider.ProviderType} is try to initialize.");
                    _isInitialize = await adProvider.InitializeAsync();

                    if (_isInitialize)
                    {
                        Verbose.D($"[ADS] : {adProvider.ProviderType} is initialized.");
                    }
                    else
                    {
                        Verbose.E($"[ADS] : {adProvider.ProviderType} is not initialized.");
                    }
                }
            }
            catch (Exception exception)
            {
                Verbose.Ex("[ADS] InitializeProviders Error", exception);
            }
        }
    }
}