using System.Security.Cryptography;
using System.Text;
using ArchitectureAI.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace ArchitectureAI.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    // AES-GCM requires a 12-byte Nonce (IV)
    private const int NonceSize = 12; 
    // Authentication Tag is 16 bytes
    private const int TagSize = 16; 

    public EncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:MasterKey"];
        if (string.IsNullOrEmpty(keyBase64))
        {
            throw new InvalidOperationException("Encryption MasterKey is missing in configuration.");
        }

        try
        {
            _key = Convert.FromBase64String(keyBase64);
            // AES-256 requires 32 bytes key
            if (_key.Length != 32)
            {
                throw new InvalidOperationException($"Encryption Key must be 32 bytes (AES-256). Current length: {_key.Length}");
            }
        }
        catch (FormatException)
        {
             throw new InvalidOperationException("Encryption MasterKey must be a valid Base64 string.");
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using var aes = new AesGcm(_key, TagSize);
        
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Format: [Nonce (12)] [Tag (16)] [CipherText (n)]
        // We combine them to store as a single string
        var combined = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, combined, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, combined, NonceSize + TagSize, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        var combined = Convert.FromBase64String(cipherText);
        
        if (combined.Length < NonceSize + TagSize)
        {
            throw new ArgumentException("Invalid encrypted data format.");
        }

        using var aes = new AesGcm(_key, TagSize);

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var cipherBytes = new byte[combined.Length - NonceSize - TagSize];

        Buffer.BlockCopy(combined, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(combined, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(combined, NonceSize + TagSize, cipherBytes, 0, cipherBytes.Length);

        var plainBytes = new byte[cipherBytes.Length];

        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
