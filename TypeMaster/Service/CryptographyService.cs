using System.Security.Cryptography;
using System.Text;
using System;

namespace TypeMaster.Service;

//not the best bcs everyone can see it in my repo
public class CryptographyService
{
    public string Encrypt(string plaintext)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string ciphertext)
    {
        byte[] encryptedBytes = Convert.FromBase64String(ciphertext);
        byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
