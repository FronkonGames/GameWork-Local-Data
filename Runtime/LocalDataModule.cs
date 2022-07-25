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

    [SerializeField, Label("Algorithm"), Indent, OnlyEnableInEdit]
    private FileIntegrity fileIntegrity = FileIntegrity.None;

    [Title("Compression")]

    [SerializeField, Label("Type"), Indent, OnlyEnableInEdit]
    private FileCompression fileCompression = FileCompression.None;

    [SerializeField, Label("Level"), Indent, OnlyEnableInEdit]
    private CompressionLevel compressionLevel = CompressionLevel.Fastest;

    [Title("Encryption")]

    [SerializeField, Label("Algorithm"), Indent, OnlyEnableInEdit]
    private FileEncryption fileEncryption = FileEncryption.None;

    [SerializeField, Indent, Password, OnlyEnableInEdit]
    private string password;

    [SerializeField, Indent, Password, OnlyEnableInEdit]
    private string seed;

    private CancellationTokenSource cancellationSource;

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
      List<FileInfo> files = new List<FileInfo>();

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
    public async void Save<T>(T localData, string file,
                              Action<int, int> onProgress = null,
                              Action<T> onEnd = null) where T : ILocalData
    {
      Check.IsNotNull(localData);
      Check.IsNotNullOrEmpty(file);
      Check.GreaterOrEqual(bufferSize, 4);

      try
      {
        cancellationSource = new();

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

          BinaryFormatter binaryFormatter = new();
          await using MemoryStream memoryStream = new();
          using (Profiling.Time($"Serializing {file}"))
          {
            binaryFormatter.Serialize(memoryStream, localData);
          }

          int uncompressedSize = (int)memoryStream.Length;

          string hash = string.Empty;
          using (Profiling.Time($"Calculating {file} integrity"))
          {
            IIntegrity integrity = fileIntegrity switch
            {
              FileIntegrity.None => new NullIntegrity(),
              FileIntegrity.MD5  => new MD5Integrity(bufferSize * 1024),
              _ => null
            };

            hash = await integrity.Calculate(memoryStream);
            writer.Write(hash);
          }

          byte[] bytes;
          using (Profiling.Time($"Compressing {file} using {fileCompression.ToString()}"))
          {
            ICompressor compressor = fileCompression switch
            {
              FileCompression.None   => new NullCompressor(),
              FileCompression.GZip   => new GZipCompressor(compressionLevel),
              _ => null
            };

            Check.IsNotNull(compressor);
            bytes = await compressor.Compress(memoryStream);
            memoryStream.Close();
          }

          using (Profiling.Time($"Encrypting {file} using {fileEncryption.ToString()}"))
          {
            IEncryptor encryptor = fileEncryption switch
            {
              FileEncryption.None => new NullEncryptor(),
              FileEncryption.AES  => new AESEncryptor(password, seed),
              _ => null
            };

            Check.IsNotNull(encryptor);
            bytes = await encryptor.Encrypt(bytes);
          }

          writer.Write(uncompressedSize);
          writer.Write(localData.Version);

          using (Profiling.Time($"Writing {fileName} data"))
          {
            await fileStream.WriteAsync(bytes);
          }
        }
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }
      finally
      {
        cancellationSource = null;

        onEnd?.Invoke(localData);
      }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="slot"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<T> Load<T>(string file, Action<int, int> onProgress = null, Action<T> onEnd = null) where T : LocalData
    {
      Check.IsNotNullOrEmpty(file);
      Check.GreaterOrEqual(bufferSize, 4);

      T data = null;

      try
      {
        cancellationSource = new CancellationTokenSource();

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
          int size = binaryReader.ReadInt32();
          int version = binaryReader.ReadInt32();
          
          await using MemoryStream stream = new();
          using (Profiling.Time($"Reading {file} data"))
          {
            int bytesRead = 0;
            do
            {
              bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
              if (bytesRead > 0)
                await stream.WriteAsync(buffer, 0, bytesRead);
            } while (bytesRead > 0);
          }
          
          using (Profiling.Time($"Checking {file} integrity"))
          {
            IIntegrity integrity = fileIntegrity switch
            {
              FileIntegrity.None => new NullIntegrity(),
              FileIntegrity.MD5  => new MD5Integrity(bufferSize * 1024),
              _ => null
            };

            bool integrityOk = await integrity.Check(stream, hash);
            
            Log.Info($"File {file} integrity {integrityOk}");
          }

          // TODO: Decrypt.
          
          // TODO: Decompress.

          if (stream.Length > 0)
          {
            BinaryFormatter binaryFormatter = new();
            stream.Seek(0, SeekOrigin.Begin);
            data = binaryFormatter.Deserialize(stream) as T;

            // TODO: Check signature.
          }
        }
        else
          Log.Error($"File {Path}{file} not found");
      }
      catch (OperationCanceledException e)
      {
        Log.Warning($"File '{file}' loading canceled.");
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }
      finally
      {
        cancellationSource = null;

        onEnd?.Invoke(data);
      }

      return data;
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

    private static string ComposePath()
    {
      string path = string.Empty;
#if (UNITY_ANDROID || UNITY_IOS)
      path = $"{Application.persistentDataPath}/";
#elif UNITY_STANDALONE_OSX
      throw new NotImplementedException("Not implemented.");
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
