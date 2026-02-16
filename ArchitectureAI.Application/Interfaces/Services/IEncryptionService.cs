namespace ArchitectureAI.Application.Interfaces.Services;

public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a plain text string using AES-GCM.
    /// </summary>
    /// <param name="plainText">The sensitive data to encrypt.</param>
    /// <returns>Base64 encoded string containing Nonce + CipherText + Tag.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts a Base64 encoded string using AES-GCM.
    /// </summary>
    /// <param name="cipherText">The encrypted data (Nonce + CipherText + Tag).</param>
    /// <returns>The original plain text.</returns>
    string Decrypt(string cipherText);
}
