﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
using System.Text;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// .
  /// </summary>
  public sealed class TripleDESEncryptor : EncryptorBase
  {
    public TripleDESEncryptor(int bufferSize, string password, string seed) : base(bufferSize, password, seed) { }
    
    protected override ICryptoTransform CreateEncryptor()
    {
      MD5CryptoServiceProvider md5CryptoServiceProvider = new();
      byte[] key = md5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(password));

      TripleDESCryptoServiceProvider tripleDESProvider = new()
      {
        Key = key,
        Mode = CipherMode.ECB,
        Padding = PaddingMode.PKCS7
      };

      return tripleDESProvider.CreateEncryptor();
    }

    protected override ICryptoTransform CreateDecryptor()
    {
      MD5CryptoServiceProvider md5CryptoServiceProvider = new();
      byte[] key = md5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(password));
      
      TripleDESCryptoServiceProvider tripleDESProvider = new()
      {
        Key = key,
        Mode = CipherMode.ECB,
        Padding = PaddingMode.PKCS7
      };

      return tripleDESProvider.CreateDecryptor();
    }
  }
}