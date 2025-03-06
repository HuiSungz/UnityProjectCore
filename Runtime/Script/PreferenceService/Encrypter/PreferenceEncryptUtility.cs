
using System.Text;

namespace ProjectCore.Preference
{
    internal static class PreferenceEncryptUtility
    {
        // 16 bytes key
        internal static readonly string KEY = "m71a12x28p94r6e5";
        internal static readonly byte[] KeyBytes = Encoding.UTF8.GetBytes(KEY);
    }
}