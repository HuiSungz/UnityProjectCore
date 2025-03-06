
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.Preference
{
    public class PreferenceSettingSO : ScriptableObject
    {
        [Serializable]
        public class PreferenceFileInfo
        {
            public string PreferenceFileName;
            public bool IsEncrypt;
        }
        
        [SerializeField] private bool _saveUseThread = true;
        [SerializeField] private bool _clearOnSaves;
        [SerializeField] private bool _autoLoad;
        [SerializeField] private float _autoSaveInterval = 0f;
        
        [Space]
        [SerializeField] private List<PreferenceFileInfo> _preferenceFileInfos;
        
        public IReadOnlyList<PreferenceFileInfo> PreferenceFileInfos => _preferenceFileInfos;
        public bool AutoLoad => _autoLoad;
        public bool SaveUseThread => _saveUseThread;
        public bool ClearOnSaves => _clearOnSaves;
        public float AutoSaveInterval => _autoSaveInterval;
    }
}