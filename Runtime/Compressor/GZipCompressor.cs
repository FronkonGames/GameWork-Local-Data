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
using System.IO.Compression;
using System.Threading.Tasks;
using FronkonGames.GameWork.Foundation;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// .
  /// </summary>
  public sealed class GZipCompressor : ICompressor
  {
    private readonly CompressionLevel compressionLevel;

    public GZipCompressor(CompressionLevel compressionLevel) => this.compressionLevel = compressionLevel;

    public async Task<MemoryStream> Compress(MemoryStream stream)
    {
      Check.IsNotNull(stream);
      
      stream.Seek(0, SeekOrigin.Begin);

      MemoryStream compressedStream = new();
      await using GZipStream gzipStream = new(compressedStream, compressionLevel, true);

      await stream.CopyToAsync(gzipStream);

      return compressedStream;
    }

    public async Task<MemoryStream> Decompress(MemoryStream stream, int originalSize)
    {
      Check.IsNotNull(stream);
      Check.Greater(originalSize, 0);

      stream.Seek(0, SeekOrigin.Begin);
      
      MemoryStream uncompressedStream = new(originalSize);
      await using GZipStream gzipStream = new(stream, CompressionMode.Decompress, true);

      await gzipStream.CopyToAsync(uncompressedStream);

      uncompressedStream.Seek(0, SeekOrigin.Begin);

      return uncompressedStream;
    }
  }
}