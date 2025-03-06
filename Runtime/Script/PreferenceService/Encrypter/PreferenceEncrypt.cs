
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace ProjectCore.Preference
{
    internal static class PreferenceEncrypt
    {
        /// <summary>
        /// Encrypts a binary data array using Rijndael (AES) algorithm with CBC mode and PKCS7 padding.
        /// Returns the encrypted byte array or null if encryption fails.
        /// </summary>
        /// <param name="rawData">The raw binary data to encrypt</param>
        /// <returns>Encrypted byte array or empty array if input is null/empty</returns>
        public static byte[] EncryptBinary(byte[] rawData)
        {
            if (rawData == null || rawData.Length == 0)
            {
                return Array.Empty<byte>();
            }

            try
            {
                using var rijndael = new RijndaelManaged();
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;
                rijndael.KeySize = 128;
                rijndael.BlockSize = 128;
                rijndael.Key = PreferenceEncryptUtility.KeyBytes;
                rijndael.IV = PreferenceEncryptUtility.KeyBytes;

                using var memoryStream = new MemoryStream();
                using (var cryptoStream = new CryptoStream(
                           memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(rawData, 0, rawData.Length);
                    cryptoStream.FlushFinalBlock();
                }

                return memoryStream.ToArray();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[PreferenceEncrypt] EncryptBinary Error: {exception.Message}");
                return null;
            }
        }

        /// <summary>
        /// Decrypts a binary data array that was encrypted using Rijndael (AES) algorithm with CBC mode and PKCS7 padding.
        /// Returns the decrypted byte array or null if decryption fails.
        /// </summary>
        /// <param name="encryptedData">The encrypted binary data to decrypt</param>
        /// <returns>Decrypted byte array or empty array if input is null/empty</returns>
        public static byte[] DecryptBinary(byte[] encryptedData)
        {
            if(encryptedData == null || encryptedData.Length == 0)
            {
                return Array.Empty<byte>();
            }

            try
            {
                using var rijndael = new RijndaelManaged();
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;
                rijndael.KeySize = 128;
                rijndael.BlockSize = 128;
                rijndael.Key = PreferenceEncryptUtility.KeyBytes;
                rijndael.IV = PreferenceEncryptUtility.KeyBytes;
                
                using var memoryStream = new MemoryStream();
                using (var cryptoStream = new CryptoStream(
                           memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(encryptedData, 0, encryptedData.Length);
                    cryptoStream.FlushFinalBlock();
                }

                return memoryStream.ToArray();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[PreferenceEncrypt] DecryptBinary Error: {exception.Message}");
                return null;
            }
        }
    }
}