
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Preference
{
    [Serializable]
    internal sealed class PreferenceService
    {
        #region Fields

        [SerializeField] private PreferenceContainer[] _prefContainers;
        private List<PreferenceContainer> _prefContainerList;
        
        public PreferenceSettingSO.PreferenceFileInfo Info { get; set; }

        #endregion
        
        public void Initialize()
        {
            _prefContainerList = _prefContainers == null 
                ? new List<PreferenceContainer>() 
                : new List<PreferenceContainer>(_prefContainers);
        }

        public void Flush()
        {
            _prefContainers = _prefContainerList.ToArray();

            foreach (var prefContainer in _prefContainerList)
            {
                prefContainer.Flush();
            }
        }
        
        public T GetPreferenceObject<T>(int hash) where T : IPreferenceObject, new()
        {
            var container = _prefContainerList.Find(container => container.Hash == hash);
            if (container != null)
            {
                if (!container.IsRestored)
                {
                    container.Restore<T>();
                }
                return (T)container.Object;
            }
            
            container = new PreferenceContainer(hash, new T());
            _prefContainerList.Add(container);

            return (T)container.Object;
        }
    }
}