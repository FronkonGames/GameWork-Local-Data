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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;
using FronkonGames.GameWork.Core;
using FronkonGames.GameWork.Foundation;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// .
  /// </summary>
  public sealed class LocalDataModule : MonoBehaviourModule,
                                        IInitializable
  {
    /// <summary>
    /// Is it initialized?
    /// </summary>
    /// <value>Value</value>
    public bool Initialized { get; set; }
    
    public string Path { get; set; }

    public bool Busy => cancellationSource != null;

    [Title("Streams")]

    [SerializeField, Label("Buffer size (KB)"), Indent, Range(1, 256), OnlyEnableInEdit]
    private int bufferSize = 32;

    [Title("Integrity")]

    [SerializeField, Label("Algorithm"), Indent]
    private FileIntegrity fileIntegrity = FileIntegrity.None;

    [Title("Compression")]

    [SerializeField, Label("Type"), Indent]
    private FileCompression fileCompression = FileCompression.None;

    [SerializeField, Label("Level"), Indent]
    private CompressionLevel compressionLevel = CompressionLevel.Fastest;

    [Title("Encryption")]

    [SerializeField, Label("Algorithm"), Indent]
    private FileEncryption fileEncryption = FileEncryption.None;

    [SerializeField, Indent, Password, OnlyEnableInEdit]
    private string password;

    [SerializeField, Indent, Password, OnlyEnableInEdit]
    private string seed;

    private CancellationTokenSource cancellationSource;

    private Action<float> clientProgress;
    private float currentProgress;
    private int progressSteps;

    /// <summary>
    /// When initialize.
    /// </summary>
    public void OnInitialize()
    {
      cancellationSource = null;

      Path = ComposePath();
      
      Log.Info($"Using path '{Path}'");
    }

    /// <summary>
    /// At the end of initialization.
    /// Called in the first Update frame.
    /// </summary>
    public void OnInitialized()
    {
    }

    /// <summary>
    /// When deinitialize.
    /// </summary>
    public void OnDeinitialize()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    public List<FileInfo> GetFilesInfo(string searchPattern = "*.*", bool recursive = false)
    {
      List<FileInfo> files = new();

      try
      {
        if (Directory.Exists(Path) == true)
        {
          string[] fileNames = Directory.GetFiles(Path, searchPattern, recursive == true ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
          for (int i = 0; i < fileNames.Length; ++i)
            files.Add(new FileInfo(fileNames[i]));
        }
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }

      return files;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="searchPattern"></param>
    /// <param name="recursive"></param>
    /// <returns></returns>
    public FileInfo GetFileInfo(string file)
    {
      Check.IsNotNullOrEmpty(file);
      FileInfo fileInfo = null;

      try
      {
        fileInfo = new FileInfo(Path + file); 
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }

      return fileInfo.Exists == true ? fileInfo : null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public bool Exists(string file)
    {
      Check.IsNotNullOrEmpty(file);
      bool success = false;

      try
      {
        if (new FileInfo(Path + file).Exists == true)
          success = true;
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }

      return success;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public string NextAvailableName(string file)
    {
      string availableName = file;

      int index = 0;
      try
      {
        string name = System.IO.Path.GetFileNameWithoutExtension(file);
        string extension = System.IO.Path.GetExtension(file);

        do
        {
          availableName = $"{name}{index:000}{extension}"; 
          index++;
        } while (Exists(availableName) == true && index < 1000);
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }
      
      if (index == 1000)
        Log.Error($"Index limit reached for file '{file}'");

      return availableName;
    }

    public void CancelAsyncOperations() => cancellationSource?.Cancel(); 

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

      onProgress?.Invoke(0.0f);

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
          // Version           : int.
          // Data              : byte[].
          
          writer.Write(localData.Signature);
          writer.Write((byte)fileIntegrity);
          writer.Write((byte)fileCompression);
          writer.Write((byte)fileEncryption);

          clientProgress = onProgress;
          currentProgress = 0.0f;
          progressSteps = 1;
          if (fileIntegrity != FileIntegrity.None)     progressSteps++;
          if (fileCompression != FileCompression.None) progressSteps++;
          if (fileEncryption != FileEncryption.None)   progressSteps++;

          IIntegrity integrity = CreateFileIntegrity(fileIntegrity);
          ICompressor compressor = CreateFileCompressor(fileCompression);
          IEncryptor encryptor = CreateFileEncryptor(fileEncryption);

#if ENABLE_PROFILING
          using (Profiling.Time($"Serializing {file}"))
#endif
          {
            BinaryFormatter binaryFormatter = new();
            binaryFormatter.Serialize(stream, localData);
          }

          int uncompressedSize = (int)stream.Length;

          string hash = string.Empty;
#if ENABLE_PROFILING
          using (Profiling.Time($"Calculating {file} integrity"))
#endif
          {
            hash = await integrity.Calculate(stream, CalculateProgress);
          }

          writer.Write(hash);

          if (fileIntegrity != FileIntegrity.None)
            currentProgress += 1.0f / progressSteps;
          
#if ENABLE_PROFILING
          using (Profiling.Time($"Compressing {file} using {fileCompression}"))
#endif
          {
            stream = await compressor.Compress(stream, CalculateProgress);
          }

          if (fileCompression != FileCompression.None)
            currentProgress += 1.0f / progressSteps;

#if ENABLE_PROFILING
          using (Profiling.Time($"Encrypting {file} using {fileEncryption}"))
#endif
          {
            stream = await encryptor.Encrypt(stream, CalculateProgress);
          }

          if (fileEncryption != FileEncryption.None)
            currentProgress += 1.0f / progressSteps;
          
          writer.Write(uncompressedSize);
          writer.Write(localData.Version);

#if ENABLE_PROFILING
          using (Profiling.Time($"Writing {fileName} data"))
#endif
          {
            byte[] data = stream.ToArray();
            for (int offset = 0; offset < data.Length; )
            {
              int length = Math.Min(bufferSize * 1024, data.Length - offset);
              await fileStream.WriteAsync(data, offset, length);
              offset += length;

              onProgress?.Invoke((float)offset / data.Length * (1.0f / progressSteps) + currentProgress);
            }
          }
        }
      }
      catch (OperationCanceledException e)
      {
        result = FileResult.Cancelled;

        Log.Warning($"File '{file}' writing canceled.");
      }
/* 
      catch (Exception e)
      {
        result = FileResult.ExceptionRaised;

        Log.Exception(e.ToString());
      }
*/
      finally
      {
        await stream.DisposeAsync();

        cancellationSource = null;

        onProgress?.Invoke(1.0f);
        
        onEnd?.Invoke(result, localData);
      }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="slot"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task Read<T>(string file,
                              Action<float> onProgress = null,
                              Action<FileResult, T> onEnd = null) where T : LocalData
    {
      Check.IsNotNullOrEmpty(file);
      Check.GreaterOrEqual(bufferSize, 4);

      FileResult result = FileResult.Ok;
      
      T data = null;
      MemoryStream stream = new();
      cancellationSource = new CancellationTokenSource();

      try
      {
        onProgress?.Invoke(0.0f);

        FileInfo fileInfo = GetFileInfo(file);
        if (fileInfo != null)
        {
          byte[] buffer = new byte[bufferSize * 1024];

          await using FileStream fileStream = new(Path + file, FileMode.Open, FileAccess.Read, FileShare.Read);
          using BinaryReader binaryReader = new(fileStream);

          string fileSignature = binaryReader.ReadString();
          FileIntegrity fileIntegrity = (FileIntegrity)binaryReader.ReadByte();
          FileCompression fileCompression = (FileCompression)binaryReader.ReadByte();
          FileEncryption fileEncryption = (FileEncryption)binaryReader.ReadByte();
          string hash = binaryReader.ReadString();
          int uncompressedSize = binaryReader.ReadInt32();
          int version = binaryReader.ReadInt32();

          int steps = 1;
          if (fileIntegrity != FileIntegrity.None)     steps++;
          if (fileCompression != FileCompression.None) steps++;
          if (fileEncryption != FileEncryption.None)   steps++;

          IIntegrity integrity = CreateFileIntegrity(fileIntegrity);
          ICompressor compressor = CreateFileCompressor(fileCompression);
          IEncryptor encryptor = CreateFileEncryptor(fileEncryption);

#if ENABLE_PROFILING
          using (Profiling.Time($"Reading {file} data"))
#endif
          {
            int bytesRead = 0;
            do
            {
              bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
              if (bytesRead > 0)
                await stream.WriteAsync(buffer, 0, bytesRead);
              
              onProgress?.Invoke(((float)stream.Length / fileStream.Length) * 0.5f);
            } while (bytesRead > 0);
          }
          
#if ENABLE_PROFILING
          using (Profiling.Time($"Checking {file} integrity"))
#endif
          {
            result = await integrity.Check(stream, hash) ? FileResult.Ok : FileResult.IntegrityFailure;

            onProgress?.Invoke(0.666f);
          }

          if (result == FileResult.Ok)
          {
#if ENABLE_PROFILING
            using (Profiling.Time($"Decrypting {file}"))
#endif
            {
              stream = await encryptor.Decrypt(stream);
            
              onProgress?.Invoke(0.832f);
            }

#if ENABLE_PROFILING
            using (Profiling.Time($"Decompressing {file}"))
#endif
            {
              stream = await compressor.Decompress(stream, uncompressedSize);
            }

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
        }
        else
          Log.Error($"File {Path}{file} not found");
      }
      catch (OperationCanceledException e)
      {
        result = FileResult.Cancelled;

        Log.Warning($"File '{file}' loading canceled.");
      }
      catch (Exception e)
      {
        result = FileResult.ExceptionRaised;

        Log.Exception(e.ToString());
      }
      finally
      {
        stream?.Dispose();
        
        cancellationSource = null;

        onProgress?.Invoke(1.0f);

        onEnd?.Invoke(result, data);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    public void Delete(string file)
    {
      Check.IsNotNullOrEmpty(file);

      if (Exists(file) == true)
      {
        try
        {
          File.Delete(Path + file);
        }
        catch (Exception e)
        {
          Log.Exception(e.ToString());
        }
      }
      else
        Log.Warning($"File '{file}' not found");
    }

    private IIntegrity CreateFileIntegrity(FileIntegrity fileIntegrity)
    {
      IIntegrity integrity = fileIntegrity switch
      {
        FileIntegrity.None => new NullIntegrity(),
        FileIntegrity.MD5  => new MD5Integrity(bufferSize),
        _ => null
      };
      Check.IsNotNull(integrity);

      return integrity;
    }

    private ICompressor CreateFileCompressor(FileCompression fileCompression)
    {
      ICompressor compressor = fileCompression switch
      {
        FileCompression.None   => new NullCompressor(),
        FileCompression.Zip    => new ZipCompressor(bufferSize, compressionLevel),
        FileCompression.GZip   => new GZipCompressor(bufferSize, compressionLevel),
        FileCompression.Brotli => new BrotliCompressor(bufferSize, compressionLevel),
        _ => null
      };
      Check.IsNotNull(compressor);

      return compressor;
    }

    private IEncryptor CreateFileEncryptor(FileEncryption fileEncryption)
    {
      IEncryptor encryptor = fileEncryption switch
      {
        FileEncryption.None => new NullEncryptor(),
        FileEncryption.AES  => new AESEncryptor(bufferSize, password, seed),
        FileEncryption.DES  => new DESEncryptor(bufferSize, password),
        _ => null
      };
      Check.IsNotNull(encryptor);

      return encryptor;
    }

    private void CalculateProgress(float progress)
    {
      Check.NotEqual(progressSteps, 0);

      clientProgress?.Invoke((progress / progressSteps + currentProgress) * 0.5f);
    }

    private static string ComposePath()
    {
      string path = string.Empty;
#if (UNITY_ANDROID || UNITY_IOS)
      path = $"{Application.persistentDataPath}/";
#elif UNITY_STANDALONE_OSX
      path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/";
#elif UNITY_STANDALONE_WIN
      path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/My Games/";
#else
      throw new NotSupportedException("platform not supported.");
#endif

      if (string.IsNullOrEmpty(Application.companyName) == false)
        path += Application.companyName.RemoveInvalidFileCharacters();

      if (string.IsNullOrEmpty(Application.productName) == false)
        path += "/" + Application.productName.RemoveInvalidFileCharacters();

      path += "/";
      path = path.Replace("\\", "/");
      path = path.Replace("//", "/");

      return path;
    }

    private bool CheckPath(string file)
    {
      bool success = false;
      try
      {
        string path = System.IO.Path.GetDirectoryName(file);
        if (Directory.Exists(path) == false)
        {
          Directory.CreateDirectory(path);

          success = true;
        }
        else
          success = true;
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }

      return success;
    }
  }
}
