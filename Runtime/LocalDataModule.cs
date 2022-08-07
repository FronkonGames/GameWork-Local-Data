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
using System.Collections.Generic;
using System.IO;
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
  public sealed partial class LocalDataModule : MonoBehaviourModule,
                                                IInitializable
  {
    /// <summary>
    /// Is it initialized?
    /// </summary>
    /// <value>Value</value>
    public bool Initialized { get; set; }
    
    public string Path { get; set; }

    public FileIntegrity Integrity { get => fileIntegrity; set => fileIntegrity = value; }

    public FileCompression Compression { get => fileCompression; set => fileCompression = value; }

    public FileEncryption Encryption { get => fileEncryption; set => fileEncryption = value; }

    public bool Busy => cancellationSource != null;

    [Title("Streams")]

    [SerializeField, Label("Buffer size (KB)"), Indent, Range(1, 256)]
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
    private int totalSteps = 0;
    private int currentStep;

    /// <summary>
    /// When initialize.
    /// </summary>
    public void OnInitialize()
    {
      cancellationSource = null;

      Path = ComposePath();
    }

    /// <summary>
    /// At the end of initialization.
    /// Called in the first Update frame.
    /// </summary>
    public void OnInitialized()
    {
      ResetProgress(null);
    }

    /// <summary>
    /// When deinitialize.
    /// </summary>
    public void OnDeinitialize()
    {
      Cancel();
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
    public string NextAvailableName(string file, string separator = "")
    {
      string availableName = file;

      int index = 0;
      try
      {
        while (Exists(availableName) == true && index < 1000)
        {
          string name = System.IO.Path.GetFileNameWithoutExtension(file);
          string extension = System.IO.Path.GetExtension(file);
          
          availableName = $"{name}{separator}{index:000}{extension}"; 
          index++;
        }
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }
      
      if (index == 1000)
        Log.Error($"Index limit reached for file '{file}'");

      return availableName;
    }

    public void Cancel() => cancellationSource?.Cancel(); 

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

    private IIntegrity CreateFileIntegrity(FileIntegrity fileIntegrity, CancellationToken cancellationToken)
    {
      IIntegrity integrity = fileIntegrity switch
      {
        FileIntegrity.None   => new NullIntegrity(),
        FileIntegrity.MD5    => new MD5Integrity(bufferSize, cancellationToken),
        FileIntegrity.SHA1   => new SHA1Integrity(bufferSize, cancellationToken),
        FileIntegrity.SHA256 => new SHA256Integrity(bufferSize, cancellationToken),
        FileIntegrity.SHA512 => new SHA512Integrity(bufferSize, cancellationToken),
        _ => null
      };
      Check.IsNotNull(integrity);

      return integrity;
    }

    private ICompressor CreateFileCompressor(FileCompression fileCompression, CancellationToken cancellationToken)
    {
      ICompressor compressor = fileCompression switch
      {
        FileCompression.None   => new NullCompressor(),
        FileCompression.Zip    => new ZipCompressor(bufferSize, compressionLevel, cancellationToken),
        FileCompression.GZip   => new GZipCompressor(bufferSize, compressionLevel, cancellationToken),
        FileCompression.Brotli => new BrotliCompressor(bufferSize, compressionLevel, cancellationToken),
        _ => null
      };
      Check.IsNotNull(compressor);

      return compressor;
    }

    private IEncryptor CreateFileEncryptor(FileEncryption fileEncryption, CancellationToken cancellationToken)
    {
      IEncryptor encryptor = fileEncryption switch
      {
        FileEncryption.None       => new NullEncryptor(),
        FileEncryption.AES        => new AESEncryptor(bufferSize, password, seed, cancellationToken),
        FileEncryption.RC2        => new RC2Encryptor(bufferSize, password, string.Empty, cancellationToken),
        FileEncryption.DES        => new DESEncryptor(bufferSize, password, string.Empty, cancellationToken),
        FileEncryption.TripleDES  => new TripleDESEncryptor(bufferSize, password, seed, cancellationToken),
        _ => null
      };
      Check.IsNotNull(encryptor);

      return encryptor;
    }

    private void CalculateProgress(float progress)
    {
      currentProgress = (progress + currentStep) / totalSteps;

      clientProgress?.Invoke(Mathf.Clamp01(currentProgress));
    }

    private void ResetProgress(Action<float> progress)
    {
      clientProgress = progress;
      currentProgress = 0.0f;
      currentStep = 0;
      totalSteps = 1;

      progress?.Invoke(0.0f);
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
