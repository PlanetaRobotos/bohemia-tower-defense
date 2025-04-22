using System;

namespace Infrastructure.Services.LocalData
{
    public interface IDataManager
    {
        /// <summary>
        /// Saves data of any type to PlayerPrefs with encryption
        /// </summary>
        /// <typeparam name="T">Type of data to save</typeparam>
        /// <param name="key">Key to save the data under</param>
        /// <param name="value">Data to save</param>
        void Save<T>(string key, T value);

        /// <summary>
        /// Loads data of any type from PlayerPrefs with decryption
        /// </summary>
        /// <typeparam name="T">Type of data to load</typeparam>
        /// <param name="key">Key to load the data from</param>
        /// <returns>Loaded data or default value if not found</returns>
        T Load<T>(string key);

        /// <summary>
        /// Checks if a key exists in PlayerPrefs
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>True if key exists, false otherwise</returns>
        bool HasKey(string key);

        /// <summary>
        /// Deletes a specific key from PlayerPrefs
        /// </summary>
        /// <param name="key">Key to delete</param>
        void DeleteKey(string key);

        /// <summary>
        /// Deletes all data from PlayerPrefs
        /// </summary>
        void DeleteAll();
    }
}