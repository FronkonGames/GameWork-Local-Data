////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of
// the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FronkonGames.GameWork.Foundation;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// .
  /// </summary>
  public sealed class AESEncryptor : EncryptorBase
  {
    private readonly byte[] Key;
    private readonly byte[] IV;

    public AESEncryptor(string password, string seed)
    {
      Check.IsNotNullOrEmpty(password);
      Check.IsNotNullOrEmpty(seed);

      Rfc2898DeriveBytes rfc = new(password, Encoding.ASCII.GetBytes(seed));
      Key = rfc.GetBytes(16);
      IV = rfc.GetBytes(16);
    }
    
    public override async Task<byte[]> Encrypt(byte[] bytes)
    {
      await using MemoryStream encryptedStream = new();
      using AesCryptoServiceProvider aesProvider = new();
      await using CryptoStream cryptoStream = new(encryptedStream, aesProvider.CreateEncryptor(Key, IV), CryptoStreamMode.Write);

      await cryptoStream.WriteAsync(bytes, 0, bytes.Length);
      
      return encryptedStream.ToArray();
    }

    public override async Task<byte[]> Decrypt(byte[] bytes) => bytes;
  }
}