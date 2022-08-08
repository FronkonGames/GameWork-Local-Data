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
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using FronkonGames.GameWork.Core;
using FronkonGames.GameWork.Foundation;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// Module for asynchronous reading / writing of local files.
  /// </summary>
  public sealed partial class LocalDataModule : MonoBehaviourModule,
                                                IInitializable
  {
    /// <summary>
    /// Reads a file asynchronously.
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <param name="onProgress">Progress of the operation, from 0 to 1.</param>
    /// <param name="onEnd">Result of the operation, with a code and the object (if the operation was successful).</param>
    /// <typeparam name="T">Object if the operation was a success.</typeparam>
    public async Task Read<T>(string fileName,
                              Action<float> onProgress = null,
                              Action<FileResult, T> onEnd = null) where T : LocalData
    {
      Check.IsNotNullOrEmpty(fileName);
      Check.GreaterOrEqual(bufferSize, 4);

      FileResult result = FileResult.Ok;
      
      T data = null;
      MemoryStream stream = new();
      cancellationSource = new CancellationTokenSource();
      CancellationToken cancellationToken = cancellationSource.Token;

      ResetProgress(onProgress);
      
      try
      {
        FileInfo fileInfo = GetFileInfo(fileName);
        if (fileInfo != null)
        {
          byte[] buffer = new byte[bufferSize * 1024];

          await using FileStream fileStream = new(Path + fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
          using BinaryReader binaryReader = new(fileStream);

          string fileSignature = binaryReader.ReadString();
          FileIntegrity fileIntegrity = (FileIntegrity)binaryReader.ReadByte();
          FileCompression fileCompression = (FileCompression)binaryReader.ReadByte();
          FileEncryption fileEncryption = (FileEncryption)binaryReader.ReadByte();
          string hash = binaryReader.ReadString();
          int uncompressedSize = binaryReader.ReadInt32();

          IIntegrity integrity = CreateFileIntegrity(fileIntegrity, cancellationToken);
          ICompressor compressor = CreateFileCompressor(fileCompression, cancellationToken);
          IEncryptor encryptor = CreateFileEncryptor(fileEncryption, cancellationToken);

          totalSteps += fileIntegrity   != FileIntegrity.None ? 1 : 0;
          totalSteps += fileCompression != FileCompression.None ? 1 : 0;
          totalSteps += fileEncryption  != FileEncryption.None ? 1 : 0;
          
#if ENABLE_PROFILING
          using (Profiling.Time($"Reading {file} data"))
#endif
          {
            int bytesRead = 0;
            do
            {
              bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
              if (bytesRead > 0)
                await stream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

              CalculateProgress((float)stream.Length / fileStream.Length);
            } while (bytesRead > 0);
          }
          currentStep++;

#if ENABLE_PROFILING
          using (Profiling.Time($"Checking {file} integrity"))
#endif
          {
            result = await integrity.Check(stream, hash, CalculateProgress) ? FileResult.Ok : FileResult.IntegrityFailure;
          }
          currentStep += fileIntegrity != FileIntegrity.None ? 1 : 0;

          if (result == FileResult.Ok)
          {
#if ENABLE_PROFILING
            using (Profiling.Time($"Decrypting {file}"))
#endif
            {
              stream = await encryptor.Decrypt(stream, CalculateProgress);
            }
            currentStep += fileEncryption != FileEncryption.None ? 1 : 0;

#if ENABLE_PROFILING
            using (Profiling.Time($"Decompressing {file}"))
#endif
            {
              stream = await compressor.Decompress(stream, uncompressedSize, CalculateProgress);
            }
            currentStep += fileCompression != FileCompression.None ? 1 : 0;

#if ENABLE_PROFILING
            using (Profiling.Time($"Deserializing {file}"))
#endif
            {
              BinaryFormatter binaryFormatter = new();
              stream.Position = 0;
              data = binaryFormatter.Deserialize(stream) as T;
            }

            result = data.Signature.Equals(fileSignature) ? FileResult.Ok : FileResult.InvalidSignature;
          }
          else
            Log.Warning($"File '{fileName}' integrity fails");
        }
        else
          Log.Error($"File '{Path}{fileName}' not found");
      }
      catch (OperationCanceledException)
      {
        result = FileResult.Cancelled;

        Log.Info($"File '{fileName}' reading canceled.");
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

        onEnd?.Invoke(result, data);
      }
    }
  }
}
