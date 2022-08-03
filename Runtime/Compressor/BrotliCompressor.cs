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
using System.IO.Compression;
using System.Threading.Tasks;
using FronkonGames.GameWork.Foundation;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// .
  /// </summary>
  public sealed class BrotliCompressor : ICompressor
  {
    private readonly byte[] buffer;
    
    private readonly CompressionLevel compressionLevel;

    public BrotliCompressor(int bufferSize, CompressionLevel compressionLevel)
    {
      Check.Greater(bufferSize, 1);

      buffer = new byte[bufferSize * 1024];

      this.compressionLevel = compressionLevel;
    }

    public async Task<MemoryStream> Compress(MemoryStream stream, Action<float> progress = null)
    {
      Check.IsNotNull(stream);

      int bytesRead, bytesReadTotal = 0;
      stream.Position = 0;

      MemoryStream compressedStream = new();
      await using BrotliStream brotliStream = new(compressedStream, compressionLevel, true);
      do
      {
        bytesRead = await stream.ReadAsync(buffer);
        if (bytesRead > 0)
          await brotliStream.WriteAsync(buffer);

        bytesReadTotal += bytesRead;
        progress?.Invoke((float)bytesReadTotal / stream.Length);
      } while (bytesRead > 0);
      
      stream.Close();
      compressedStream.Position = 0;

      return compressedStream;
    }

    public async Task<MemoryStream> Decompress(MemoryStream stream, int originalSize, Action<float> progress = null)
    {
      Check.IsNotNull(stream);
      Check.Greater(originalSize, 0);
      
      int bytesRead, bytesReadTotal = 0;
      stream.Position = 0;

      MemoryStream uncompressedStream = new(originalSize);
      await using BrotliStream brotliStream = new(stream, CompressionMode.Decompress, true);
      do
      {
        bytesRead = await brotliStream.ReadAsync(buffer, 0, buffer.Length);
        if (bytesRead > 0)
          await uncompressedStream.WriteAsync(buffer, 0, buffer.Length);

        bytesReadTotal += bytesRead;
        progress?.Invoke((float)bytesReadTotal / stream.Length);
      } while (bytesRead > 0);

      stream.Close();
      uncompressedStream.Position = 0;

      return uncompressedStream;
    }
  }
}