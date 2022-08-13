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
using System.Security.Cryptography;
using System.Threading;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// AES encryptor.
  /// </summary>
  public sealed class AESEncryptor : EncryptorBase
  {
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="bufferSize">Buffer size, in kB</param>
    /// <param name="password">Password, must be 16 characters or greater.</param>
    /// <param name="seed">Seed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public AESEncryptor(int bufferSize, string password, string seed, CancellationToken cancellationToken)
      : base(bufferSize, password, seed, cancellationToken)
    {
      Rfc2898DeriveBytes rfc = new(password, IV);
      byte[] key = rfc.GetBytes(16);
      byte[] iv = rfc.GetBytes(16);
      
      AesCryptoServiceProvider aesProvider = new();
      Encryptor = aesProvider.CreateEncryptor(key, iv);
      Decryptor = aesProvider.CreateDecryptor(key, iv);
    }
  }
}