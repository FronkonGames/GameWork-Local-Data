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
using System.Threading;
using System.Threading.Tasks;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// File integrity calculation base.
  /// </summary>
  public abstract class IntegrityBase : IIntegrity
  {
    private readonly byte[] buffer;

    private readonly CancellationToken cancellationToken;

    /// <summary>
    /// Calculation of the integrity of a stream.
    /// </summary>
    /// <param name="stream">Memory stream.</param>
    /// <param name="progress">Progress of the calculation, from 0 to 1.</param>
    /// <returns>Hash.</returns>
    public async Task<string> Calculate(MemoryStream stream, Action<float> progress)
    {
      stream.Position = 0;
      
      int bytesRead, bytesReadTotal = 0;
      using HashAlgorithm hashAlgorithm = CreateHashAlgorithm();
      do
      {
        bytesRead = await stream.ReadAsync(buffer, 0 , buffer.Length, cancellationToken);
        if (bytesRead > 0)
        {
          hashAlgorithm.TransformBlock(buffer, 0, bytesRead, null, 0);

          bytesReadTotal += bytesRead;
          progress?.Invoke((float)bytesReadTotal / stream.Length);
        }
      } while (bytesRead > 0 && cancellationToken.IsCancellationRequested == false);

      hashAlgorithm.TransformFinalBlock(buffer, 0, 0);

      progress?.Invoke(1.0f);
      stream.Position = 0;

      return BitConverter.ToString(hashAlgorithm.Hash).Replace("-", "").ToUpperInvariant();
    }

    /// <summary>
    /// Checks the integrity of a stream.
    /// </summary>
    /// <param name="stream">Memory stream to check.</param>
    /// <param name="hash">Hash.</param>
    /// <param name="progress">Progress of the calculation, from 0 to 1.</param>
    /// <returns>True if the integrity of the stream is correct.</returns>
    public async Task<bool> Check(MemoryStream stream, string hash, Action<float> progress)
    {
      string streamHash = await Calculate(stream, progress);

      return streamHash.Equals(hash);
    }

    protected IntegrityBase(int bufferSize, CancellationToken cancellationToken)
    {
      Foundation.Check.Greater(bufferSize, 1);

      buffer = new byte[bufferSize * 1024];

      this.cancellationToken = cancellationToken;
    }

    protected abstract HashAlgorithm CreateHashAlgorithm();

  }
}