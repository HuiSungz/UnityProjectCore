
namespace ProjectCore.Preference
{
    public interface IPreferenceObject
    {
        string AssociatedPreferenceKey { get; set; }
        void Initialize();
        void Flush();
        void OnChanged();
    }
}
