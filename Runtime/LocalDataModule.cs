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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;
using FronkonGames.GameWork.Core;
using FronkonGames.GameWork.Foundation;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// Module for asynchronous reading / writing of local files.
  /// </summary>
  public sealed partial class LocalDataModule : MonoBehaviourModule,
                                                IInitializable
  {
    /// <summary>
    /// Is it initialized?
    /// </summary>
    /// <value>Value</value>
    public bool Initialized { get; set; }

    /// <summary>
    /// Working path.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Hash algorithm to calculate file integrity (default None).
    /// </summary>
    public FileIntegrity Integrity { get => fileIntegrity; set => fileIntegrity = value; }

    /// <summary>
    /// Algorithm to compress the file (default None).
    /// </summary>
    public FileCompression Compression { get => fileCompression; set => fileCompression = value; }

    /// <summary>
    /// Algorithm to encrypt the file.
    /// </summary>
    public FileEncryption Encryption { get => fileEncryption; set => fileEncryption = value; }

    /// <summary>
    /// Any operation in progress?
    /// </summary>
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
    private int totalSteps;
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
    /// All files in the working path.
    /// </summary>
    /// <param name="searchPattern">Search pattern.</param>
    /// <param name="recursive">Recursive search.</param>
    /// <returns>List with information on each file.</returns>
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
    /// Information about a file in the working directory.
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <returns>File information or null if it does not exist.</returns>
    public FileInfo GetFileInfo(string fileName)
    {
      Check.IsNotNullOrEmpty(fileName);
      FileInfo fileInfo = null;

      try
      {
        fileInfo = new FileInfo(Path + fileName); 
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }

      return fileInfo.Exists ? fileInfo : null;
    }

    /// <summary>
    /// Does the file exist in the working directory?
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <returns>True / false.</returns>
    public bool Exists(string fileName)
    {
      Check.IsNotNullOrEmpty(fileName);
      bool exists = false;

      try
      {
        if (new FileInfo(Path + fileName).Exists == true)
          exists = true;
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }

      return exists;
    }

    /// <summary>
    /// Returns the following name for a file that is available, appending three digits if necessary.
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <param name="separator">Separator between name and digits.</param>
    /// <returns>File name.</returns>
    public string NextAvailableName(string fileName, string separator = "")
    {
      string availableName = fileName;

      int index = 0;
      try
      {
        while (Exists(availableName) == true && index < 1000)
        {
          string name = System.IO.Path.GetFileNameWithoutExtension(fileName);
          string extension = System.IO.Path.GetExtension(fileName);
          
          availableName = $"{name}{separator}{index:000}{extension}"; 
          index++;
        }
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }
      
      if (index == 1000)
        Log.Error($"Index limit reached for file '{fileName}'");

      return availableName;
    }

    /// <summary>
    /// If it exists, cancels the current file operation.
    /// </summary>
    public void Cancel() => cancellationSource?.Cancel(); 

    /// <summary>
    /// Deletes a file from the working path.
    /// </summary>
    /// <param name="fileName"></param>
    public void Delete(string fileName)
    {
      Check.IsNotNullOrEmpty(fileName);

      if (Exists(fileName) == true)
      {
        try
        {
          File.Delete(Path + fileName);
        }
        catch (Exception e)
        {
          Log.Exception(e.ToString());
        }
      }
      else
        Log.Warning($"File '{fileName}' not found");
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
        FileEncryption.RC2        => new RC2Encryptor(bufferSize, password, seed, cancellationToken),
        FileEncryption.DES        => new DESEncryptor(bufferSize, password, seed, cancellationToken),
        FileEncryption.TripleDES  => new TripleDESEncryptor(bufferSize, password, seed, cancellationToken),
        _ => null
      };
      Check.IsNotNull(encryptor);

      return encryptor;
    }

    private static void AddSerializationSurrogates(IFormatter binaryFormatter)
    {
      SurrogateSelector surrogateSelector = new();
      surrogateSelector.AddSurrogate(typeof(Bounds),     new StreamingContext(StreamingContextStates.All), new BoundsSerializationSurrogate());
      surrogateSelector.AddSurrogate(typeof(Rect),       new StreamingContext(StreamingContextStates.All), new RectSerializationSurrogate());
      surrogateSelector.AddSurrogate(typeof(LayerMask),  new StreamingContext(StreamingContextStates.All), new LayerMaskSerializationSurrogate());
      surrogateSelector.AddSurrogate(typeof(Color),      new StreamingContext(StreamingContextStates.All), new ColorSerializationSurrogate());
      surrogateSelector.AddSurrogate(typeof(Vector2Int), new StreamingContext(StreamingContextStates.All), new Vector2IntSerializationSurrogate());
      surrogateSelector.AddSurrogate(typeof(Vector3Int), new StreamingContext(StreamingContextStates.All), new Vector2IntSerializationSurrogate());
      surrogateSelector.AddSurrogate(typeof(Vector2),    new StreamingContext(StreamingContextStates.All), new Vector2SerializationSurrogate());
      surrogateSelector.AddSurrogate(typeof(Vector3),    new StreamingContext(StreamingContextStates.All), new Vector3SerializationSurrogate());
      surrogateSelector.AddSurrogate(typeof(Vector4),    new StreamingContext(StreamingContextStates.All), new Vector4SerializationSurrogate());
      surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), new QuaternionSerializationSurrogate());
      surrogateSelector.AddSurrogate(typeof(Matrix4x4),  new StreamingContext(StreamingContextStates.All), new Matrix4x4SerializationSurrogate());
      
      binaryFormatter.SurrogateSelector = surrogateSelector; 
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
