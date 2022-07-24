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

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// .
  /// </summary>
  public sealed class GZipCompressor : CompressorBase
  {
    private readonly CompressionLevel compressionLevel;

    public GZipCompressor(CompressionLevel compressionLevel) => this.compressionLevel = compressionLevel;

    public override async Task<byte[]> Compress(MemoryStream stream)
    {
      await using MemoryStream outStream = new();
      await using GZipStream gZipCompressor = new(outStream, compressionLevel, false);

      byte[] bytes = stream.ToArray();
      await gZipCompressor.WriteAsync(bytes, 0, bytes.Length);

      return outStream.ToArray();
    }

    public override async Task<byte[]> Decompress(MemoryStream memoryStream, byte[] buffer, int originalSize)
    {
      await using GZipStream gZipCompressor = new(memoryStream, CompressionMode.Decompress);

      await using MemoryStream outStream = new();
      int bytesRead = 0;
      do
      {
        bytesRead = await gZipCompressor.ReadAsync(buffer, 0, buffer.Length);
        if (bytesRead > 0)
          await outStream.WriteAsync(buffer, 0, bytesRead);
      } while (bytesRead > 0);

      return memoryStream.ToArray();
    }
  }
}