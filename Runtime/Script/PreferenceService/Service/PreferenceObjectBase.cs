
using System;

namespace ProjectCore.Preference
{
    [Serializable]
    public class PreferenceObjectBase : IPreferenceObject
    {
        [NonSerialized] private string _associatedPreferenceKey;
        
        public string AssociatedPreferenceKey
        {
            get => _associatedPreferenceKey;
            set => _associatedPreferenceKey = value;
        }

        protected PreferenceObjectBase(string fileName)
        {
            _associatedPreferenceKey = fileName;
        }

        public virtual void Initialize() { }

        public virtual void Flush() { }

        public virtual void OnChanged()
        {
            PreferenceController.MarkAsSaveIsRequired(_associatedPreferenceKey);
        }
    }
}