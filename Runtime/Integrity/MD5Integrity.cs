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
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// .
  /// </summary>
  public sealed class MD5Integrity : IIntegrity
  {
    private readonly byte[] buffer;

    public MD5Integrity(int bufferSize)
    {
      Foundation.Check.Greater(bufferSize, 1024);

      buffer = new byte[bufferSize];
    }

    public async Task<string> Calculate(MemoryStream stream, Action<float> progress = null)
    {
      int bytesRead, bytesReadTotal = 0;

      stream.Seek(0, SeekOrigin.Begin);
      
      using MD5 md5 = MD5.Create();
      do
      {
        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        if (bytesRead > 0)
          md5.TransformBlock(buffer, 0, bytesRead, null, 0);

        bytesReadTotal += bytesRead;
        progress?.Invoke((float)bytesReadTotal / stream.Length);
      } while (bytesRead > 0);

      md5.TransformFinalBlock(buffer, 0, 0);
      
      stream.Seek(0, SeekOrigin.Begin);

      return BitConverter.ToString(md5.Hash).Replace("-", "").ToUpperInvariant();
    }

    public async Task<bool> Check(MemoryStream stream, string hash, Action<float> progress = null)
    {
      string streamHash = await Calculate(stream);

      return streamHash.Equals(hash);
    }
  }
}