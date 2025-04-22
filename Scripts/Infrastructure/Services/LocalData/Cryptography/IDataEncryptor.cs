namespace Infrastructure.Services.LocalData.Cryptography
{
    public interface IDataEncryptor
    {
        /// <summary>
        /// Encrypts the given string data by private secret key and returns encrypted result.
        /// </summary>
        /// <param name="value">The raw string data.</param>
        /// <param name="key">The secret key.</param>
        /// <returns>Encrypted Base64 string.</returns>
        string Encrypt(string value, string key);
        
        /// <summary>
        /// Decrypts the given Base64 data by private secret key and returns decrypted result.
        /// </summary>
        /// <param name="encryptedValue">Base64 encrypted data.</param>
        /// <param name="key">The secret key.</param>
        /// <returns>The decrypted raw data.</returns>
        string Decrypt(string encryptedValue, string key);
    }
}