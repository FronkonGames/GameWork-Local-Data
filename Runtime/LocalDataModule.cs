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
using UnityEngine;
using FronkonGames.GameWork.Core;
using FronkonGames.GameWork.Foundation;
using UnityEditor.Experimental.GraphView;

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
    
    [Title("Settings")]

    [SerializeField, Label("Compress data"), Indent, OnlyEnableInEdit]
    private bool compress;

    [SerializeField, Label("Encrypt data"), Indent, OnlyEnableInEdit]
    private bool encrypted;

    [Title("Streams")]

    [SerializeField, Label("Buffer size (KB)"), Indent, Range(1, 32), OnlyEnableInEdit]
    private int bufferSize = 4;

    /// <summary>
    /// When initialize.
    /// </summary>
    public void OnInitialize()
    {
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
    public List<FileInfo> Files(string searchPattern = "*.*", bool recursive = false)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="slot"></param>
    /// <typeparam name="T"></typeparam>
    public async void Save<T>(T data, string file, Action<float> onProgress = null, Action onEnd = null) where T : LocalFile
    {
      Check.IsNotNull(data);
      Check.IsNotNullOrEmpty(file);
      Check.GreaterOrEqual(bufferSize, 4);

      try
      {
        file = Path + file;
        if (CheckPath(file) == true)
        {
          using (FileStream fileStream = new FileStream(file,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize * 1024,
            true))
          {
            byte[] bytes = ToBytes(data);
        
            await fileStream.WriteAsync(bytes, 0, bytes.Length);
          }
        }
        
        onEnd?.Invoke();
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="slot"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<T> Load<T>(string file) where T : LocalFile
    {
      Check.IsNotNullOrEmpty(file);
      Check.GreaterOrEqual(bufferSize, 4);

      T data = null;

      try
      {
        file = Path + file;
        if (Exists(file) == true)
        {
          using (FileStream sourceStream = new FileStream(file,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize * 1024,
            true))
          {
            byte[] buffer = new byte[bufferSize * 1024];

            await sourceStream.ReadAsync(buffer, 0, buffer.Length);

            data = FromBytes<T>(buffer);
          }
        }
        else
          Log.Error($"File {file} not found");
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
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
    
    private byte[] ToBytes<T>(T data) where T : LocalFile 
    {
      BinaryFormatter binaryFormatter = new BinaryFormatter();
      using (MemoryStream memoryStream = new MemoryStream())
      {
        binaryFormatter.Serialize(memoryStream, data);
        return memoryStream.ToArray();
      }
    }

    private T FromBytes<T>(byte[] data) where T : LocalFile
    {
      using (MemoryStream memoryStream = new MemoryStream(data))
      {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        return (T)binaryFormatter.Deserialize(memoryStream);
      }
    }    
  }
}
