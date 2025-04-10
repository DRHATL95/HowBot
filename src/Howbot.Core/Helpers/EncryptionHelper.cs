﻿using System.Security.Cryptography;
using System.Text;

namespace Howbot.Core.Helpers;

public static class StringCipher
{
  // This constant is used to determine the keysize of the encryption algorithm in bits.
  // We divide this by 8 within the code below to get the equivalent number of bytes.
  private const int Keysize = 256;

  // This constant determines the number of iterations for the password bytes generation function.
  private const int DerivationIterations = 1000;

  public static string Encrypt(string plainText, string passPhrase)
  {
    // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
    // so that the same Salt and IV values can be used when decrypting.
    var saltStringBytes = Generate256BitsOfRandomEntropy();
    var ivStringBytes = Generate128BitsOfRandomEntropy();
    var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

    using var password =
      new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations, HashAlgorithmName.SHA256);
    var keyBytes = password.GetBytes(Keysize / 8);

    using var symmetricKey = Aes.Create();
    symmetricKey.KeySize = 256;
    symmetricKey.BlockSize = 128;
    symmetricKey.Mode = CipherMode.CBC;
    symmetricKey.Padding = PaddingMode.PKCS7;

    using var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes);

    using var memoryStream = new MemoryStream();

    using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
    cryptoStream.FlushFinalBlock();

    // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
    var cipherTextBytes = saltStringBytes;
    cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
    cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();

    memoryStream.Close();
    cryptoStream.Close();

    return Convert.ToBase64String(cipherTextBytes);
  }

  public static string Decrypt(string cipherText, string passPhrase)
  {
    // Get the complete stream of bytes that represent:
    // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
    var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
    // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
    var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
    // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
    var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(16).ToArray();
    // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
    var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) + 16)
      .Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) + 16)).ToArray();

    using var password =
      new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations, HashAlgorithmName.SHA256);
    var keyBytes = password.GetBytes(Keysize / 8);

    using var symmetricKey = Aes.Create();
    symmetricKey.KeySize = 256;
    symmetricKey.BlockSize = 128;
    symmetricKey.Mode = CipherMode.CBC;
    symmetricKey.Padding = PaddingMode.PKCS7;

    using var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes);

    using var memoryStream = new MemoryStream(cipherTextBytes);

    using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

    using var streamReader = new StreamReader(cryptoStream, Encoding.UTF8);

    return streamReader.ReadToEnd();
  }

  private static byte[] Generate128BitsOfRandomEntropy()
  {
    var randomBytes = new byte[16];
    RandomNumberGenerator.Fill(randomBytes);
    return randomBytes;
  }

  private static byte[] Generate256BitsOfRandomEntropy()
  {
    var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
    // Fill the array with cryptographically secure random bytes.
    RandomNumberGenerator.Fill(randomBytes);
    return randomBytes;
  }
}
