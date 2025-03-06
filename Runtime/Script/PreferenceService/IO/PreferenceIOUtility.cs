
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace ProjectCore.Preference
{
    internal static class PreferenceIOUtility
    {
        public static string GetEnsurePath(ref string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }
            
            path = Application.persistentDataPath;
            return path;
        }

        public static T DeserializeFromBytes<T>(byte[] data) where T : new()
        {
            if (data == null || data.Length == 0)
            {
                return new T();
            }

            try
            {
                using var memoryStream = new MemoryStream(data);
                var binaryFormatter = new BinaryFormatter();
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[PreferenceFileUtility] DeserializeFromBytes Error: {exception.Message}");
                return new T();
            }
        }

        public static byte[] SerializeToBytes<T>(T obj)
        {
            using var memoryStream = new MemoryStream();
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, obj);
            
            return memoryStream.ToArray();
        }
    }
}