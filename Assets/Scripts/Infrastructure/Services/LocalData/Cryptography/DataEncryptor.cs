using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Infrastructure.Services.LocalData.Cryptography
{
    public class DataEncryptor : IDataEncryptor
    {
        private static readonly byte[] Salt = Encoding.ASCII.GetBytes(DataConstants.Salt);

        /// <inheritdoc cref="IDataEncryptor"/>
        public string Encrypt(string value, string key)
        {
            if (string.IsNullOrEmpty(value)) return null;
            
            byte[] encrypted;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Salt;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] plainBytes = Encoding.UTF8.GetBytes(value);
                encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }

            return Convert.ToBase64String(encrypted);
        }

        /// <inheritdoc cref="IDataEncryptor"/>
        public string Decrypt(string encryptedValue, string key)
        {
            if (string.IsNullOrEmpty(encryptedValue)) return null;
            
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedValue);
                string decrypted;
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = Salt;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    decrypted = Encoding.UTF8.GetString(decryptedBytes);
                }
                return decrypted;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error decrypting data: {e.Message}");
                return null;
            }
        }
    }
}