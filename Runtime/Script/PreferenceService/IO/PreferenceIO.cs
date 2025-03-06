
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace ProjectCore.Preference
{
    internal static class PreferenceIO
    {
        #region Fields

        private static string _persistentDataPath;

        #endregion

        public static void Initialize()
        {
            _persistentDataPath = Application.persistentDataPath;
        }
        
        public static PreferenceService Deserialize(PreferenceSettingSO.PreferenceFileInfo info)
        {
            var absolutePath = Path.Combine(PreferenceIOUtility.GetEnsurePath(ref _persistentDataPath), info.PreferenceFileName);
            if (!File.Exists(absolutePath))
            {
                return GenerateAndCachingPS(info);
            }

            try
            {
                if (info.IsEncrypt)
                {
                    var encryptedData = File.ReadAllBytes(absolutePath);
                    var decryptedData = PreferenceEncrypt.DecryptBinary(encryptedData);

                    if (decryptedData != null && decryptedData.Length != 0)
                    {
                        var preferenceService = PreferenceIOUtility.DeserializeFromBytes<PreferenceService>(decryptedData);
                        preferenceService.Info = info;
                        return preferenceService;
                    }
                    
                    Debug.LogError($"[PreferenceFileIO] Failed to decrypt file: {absolutePath}");
                    return GenerateAndCachingPS(info);
                }

                using var fileStream = File.Open(absolutePath, FileMode.Open);
                var binaryFormatter = new BinaryFormatter();
                return (PreferenceService)binaryFormatter.Deserialize(fileStream);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[PreferenceFileIO] Deserialize Error: {exception.Message}");
                return GenerateAndCachingPS(info);
            }
        }

        public static bool Serialize(PreferenceService preferenceService)
        {
            var absolutePath = Path.Combine(PreferenceIOUtility.GetEnsurePath(ref _persistentDataPath), preferenceService.Info.PreferenceFileName);
            try
            {
                if (preferenceService.Info.IsEncrypt)
                {
                    var serializedData = PreferenceIOUtility.SerializeToBytes(preferenceService);
                    var encryptedData = PreferenceEncrypt.EncryptBinary(serializedData);

                    if (encryptedData == null)
                    {
                        Debug.LogError($"[PreferenceFileIO] Failed to encrypt file: {absolutePath}");
                        return false;
                    }
                    
                    File.WriteAllBytes(absolutePath, encryptedData);
                }
                else
                {
                    using var fileStream = File.Open(absolutePath, FileMode.Create);
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fileStream, preferenceService);
                }
                
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[PreferenceFileIO] Serialize Error: {exception.Message}");
                return false;
            }
        }
        
        public static bool DeleteFile(string fileName)
        {
            try
            {
                var absolutePath = Path.Combine(PreferenceIOUtility.GetEnsurePath(ref _persistentDataPath), fileName);

                if (!File.Exists(absolutePath))
                {
                    return false;
                }
                
                File.Delete(absolutePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PreferenceFileIO] 파일 삭제 오류: {ex.Message}");
                return false;
            }
        }

        private static PreferenceService GenerateAndCachingPS(PreferenceSettingSO.PreferenceFileInfo info)
        {
            var preferenceService = new PreferenceService
            {
                Info = info
            };
            return preferenceService;
        }
    }
}