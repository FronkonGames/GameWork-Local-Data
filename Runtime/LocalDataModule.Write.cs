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
//#define ENABLE_PROFILING
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using FronkonGames.GameWork.Core;
using FronkonGames.GameWork.Foundation;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// .
  /// </summary>
  public sealed partial class LocalDataModule : MonoBehaviourModule,
                                                IInitializable
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="localData"></param>
    /// <param name="file"></param>
    /// <param name="onProgress"></param>
    /// <param name="onEnd"></param>
    /// <typeparam name="T"></typeparam>
    public async void Write<T>(T localData, string file,
                               Action<float> onProgress = null,
                               Action<FileResult, T> onEnd = null) where T : ILocalData
    {
      Check.IsNotNull(localData);
      Check.IsNotNullOrEmpty(file);
      Check.GreaterOrEqual(bufferSize, 4);

      FileResult result = FileResult.Ok;

      MemoryStream stream = new();
      cancellationSource = new();
      CancellationToken cancellationToken = cancellationSource.Token;

      ResetProgress(onProgress);

      try
      {
        string fileName = Path + file;
        if (CheckPath(fileName) == true)
        {
          await using FileStream fileStream = new(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
          await using BinaryWriter writer = new(fileStream);

          // Signature         : string.
          // Integrity         : byte.
          // Compression       : byte.
          // Encryption        : byte.
          // Hash              : string.
          // Uncompressed size : int.
          // Data              : byte[].
          
          writer.Write(localData.Signature);
          writer.Write((byte)fileIntegrity);
          writer.Write((byte)fileCompression);
          writer.Write((byte)fileEncryption);

          IIntegrity integrity = CreateFileIntegrity(fileIntegrity, cancellationToken);
          ICompressor compressor = CreateFileCompressor(fileCompression, cancellationToken);
          IEncryptor encryptor = CreateFileEncryptor(fileEncryption, cancellationToken);

          totalSteps += fileIntegrity   != FileIntegrity.None ? 1 : 0;
          totalSteps += fileCompression != FileCompression.None ? 1 : 0;
          totalSteps += fileEncryption  != FileEncryption.None ? 1 : 0;

#if ENABLE_PROFILING
          using (Profiling.Time($"Serializing {file}"))
#endif
          {
            BinaryFormatter binaryFormatter = new();
            binaryFormatter.Serialize(stream, localData);
          }

          int uncompressedSize = (int)stream.Length;

#if ENABLE_PROFILING
          using (Profiling.Time($"Compressing {file} using {fileCompression}"))
#endif
          {
            stream = await compressor.Compress(stream, CalculateProgress);
          }
          currentStep += fileCompression != FileCompression.None ? 1 : 0;

#if ENABLE_PROFILING
          using (Profiling.Time($"Encrypting {file} using {fileEncryption}"))
#endif
          {
            stream = await encryptor.Encrypt(stream, CalculateProgress);
          }
          currentStep += fileEncryption != FileEncryption.None ? 1 : 0;

          string hash = string.Empty;
#if ENABLE_PROFILING
          using (Profiling.Time($"Calculating {file} integrity"))
#endif
          {
            hash = await integrity.Calculate(stream, CalculateProgress);
          }
          currentStep += fileIntegrity != FileIntegrity.None ? 1 : 0;

          writer.Write(hash);
          writer.Write(uncompressedSize);

#if ENABLE_PROFILING
          using (Profiling.Time($"Writing {fileName} data"))
#endif
          {
            byte[] data = stream.ToArray();
            for (int offset = 0; offset < data.Length; )
            {
              int length = Math.Min(bufferSize * 1024, data.Length - offset);
              await fileStream.WriteAsync(data, offset, length, cancellationToken);
              offset += length;

              CalculateProgress((float)offset / data.Length);
            }
          }
        }
      }
      catch (OperationCanceledException)
      {
        result = FileResult.Cancelled;

        if (Exists(file) == true)
          Delete(file);

        Log.Info($"File '{file}' writing canceled.");
      }
      catch (Exception e)
      {
        result = FileResult.ExceptionRaised;

        Log.Exception(e.ToString());
      }
      finally
      {
        await stream.DisposeAsync();

        cancellationSource.Dispose();
        cancellationSource = null;
        clientProgress = null;

        onProgress?.Invoke(1.0f);
        
        onEnd?.Invoke(result, localData);
      }
    }
  }
}
