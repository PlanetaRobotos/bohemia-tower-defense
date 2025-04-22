using System;
using UnityEngine;
using Infrastructure.Services.LocalData.Cryptography;

namespace Infrastructure.Services.LocalData
{
    public class DataManager : IDataManager
    {
        private IDataEncryptor _encryptor;
        
        public DataManager()
        {
            _encryptor = new DataEncryptor();
        }

        public void Save<T>(string key, T value)
        {
            if (value == null)
            {
                PlayerPrefs.DeleteKey(key);
                return;
            }

            string json = JsonUtility.ToJson(value);
            var encryptedData = _encryptor.Encrypt(json, DataConstants.PrivateKey);
            PlayerPrefs.SetString(key, encryptedData);
            PlayerPrefs.Save();
        }

        public T Load<T>(string key)
        {
            string encryptedValue = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(encryptedValue))
                return default;

            string decryptedValue = _encryptor.Decrypt(encryptedValue, DataConstants.PrivateKey);
            if (string.IsNullOrEmpty(decryptedValue))
                return default;

            try
            {
                return JsonUtility.FromJson<T>(decryptedValue);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deserializing data for key {key}: {e.Message}");
                return default;
            }
        }

        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}