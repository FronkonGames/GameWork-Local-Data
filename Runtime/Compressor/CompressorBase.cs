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
  public abstract class CompressorBase : ICompressor
  {
    private readonly byte[] buffer;
    
    protected readonly CompressionLevel compressionLevel;

    protected MemoryStream memoryStream;

    protected CompressorBase(int bufferSize, CompressionLevel compressionLevel)
    {
      Check.Greater(bufferSize, 1);

      buffer = new byte[bufferSize * 1024];

      this.compressionLevel = compressionLevel;
    }

    protected abstract Stream CreateCompressorStream();

    protected abstract Stream CreateDecompressorStream(MemoryStream stream);

    public async Task<MemoryStream> Compress(MemoryStream stream, Action<float> progress = null)
    {
      Check.IsNotNull(stream);

      int bytesRead, bytesReadTotal = 0;
      stream.Position = 0;

      memoryStream = new();
      await using Stream compressorStream = CreateCompressorStream();
      do
      {
        bytesRead = await stream.ReadAsync(buffer);
        if (bytesRead > 0)
          await compressorStream.WriteAsync(buffer);

        bytesReadTotal += bytesRead;
        progress?.Invoke((float)bytesReadTotal / stream.Length);
      } while (bytesRead > 0);
      
      //stream.Close();
      memoryStream.Position = 0;

      return memoryStream;
    }

    public async Task<MemoryStream> Decompress(MemoryStream stream, int originalSize, Action<float> progress = null)
    {
      Check.IsNotNull(stream);
      Check.Greater(originalSize, 0);
      
      int bytesRead, bytesReadTotal = 0;
      stream.Position = 0;

      memoryStream = new(originalSize);
      await using Stream decompressorStream = CreateDecompressorStream(stream);
      do
      {
        bytesRead = await decompressorStream.ReadAsync(buffer, 0, buffer.Length);
        if (bytesRead > 0)
          await memoryStream.WriteAsync(buffer, 0, buffer.Length);

        bytesReadTotal += bytesRead;
        progress?.Invoke((float)bytesReadTotal / stream.Length);
      } while (bytesRead > 0);

      //stream.Close();
      memoryStream.Position = 0;

      return memoryStream;
    }
  }
}