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
using System;
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
  public sealed class DESEncryptor : IEncryptor
  {
    private readonly byte[] buffer;
    
    private readonly byte[] Key;

    public DESEncryptor(int bufferSize, string password)
    {
      Check.Greater(bufferSize, 1);
      Check.IsNotNullOrEmpty(password);

      buffer = new byte[bufferSize * 1024];

      Key = Encoding.ASCII.GetBytes(password);
    }
    
    public async Task<MemoryStream> Encrypt(MemoryStream stream, Action<float> progress = null)
    {
      Check.IsNotNull(stream);

      stream.Position = 0;

      MemoryStream encryptedStream = new();
      using DESCryptoServiceProvider desProvider = new();
      await using CryptoStream cryptoStream = new(encryptedStream, desProvider.CreateEncryptor(Key, Key), CryptoStreamMode.Write);

      int bytesRead;
      int bytesReadTotal = 0;
      do
      {
        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        if (bytesRead > 0)
          await cryptoStream.WriteAsync(buffer, 0, bytesRead);
        
        bytesReadTotal += bytesRead;
        progress?.Invoke((float)bytesReadTotal / stream.Length);
      } while (bytesRead > 0);

      cryptoStream.FlushFinalBlock();

      progress?.Invoke(1.0f);
      
      return encryptedStream;
    }

    public async Task<MemoryStream> Decrypt(MemoryStream stream, Action<float> progress = null)
    {
      Check.IsNotNull(stream);

      stream.Position = 0;

      MemoryStream decryptedStream = new();
      await using MemoryStream encryptedStream = new(stream.ToArray());
      using DESCryptoServiceProvider desProvider = new();
      await using CryptoStream cryptoStream = new(encryptedStream, desProvider.CreateDecryptor(Key, Key), CryptoStreamMode.Read);

      int bytesRead;
      int bytesReadTotal = 0;
      do
      {
        bytesRead = await cryptoStream.ReadAsync(buffer, 0, buffer.Length);
        if (bytesRead > 0)
          await decryptedStream.WriteAsync(buffer, 0, bytesRead);

        bytesReadTotal += bytesRead;
        progress?.Invoke((float)bytesReadTotal / stream.Length);
      } while (bytesRead > 0);

      progress?.Invoke(1.0f);
      
      return decryptedStream;
    }
  }
}