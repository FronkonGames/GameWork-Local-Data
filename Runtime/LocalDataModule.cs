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

    [SerializeField, Indent, Tooltip("Buffer size, in KB."), Range(1, 32), OnlyEnableInEdit]
    private int bufferSize = 4;

    [SerializeField, Tooltip("Use compressed data."), OnlyEnableInEdit, Indent]
    private bool compress;

    [SerializeField, Tooltip("Use encrypted data."), OnlyEnableInEdit, Indent]
    private bool encrypted;

    /// <summary>
    /// When initialize.
    /// </summary>
    public void OnInitialize()
    {
      if (string.IsNullOrEmpty(Application.companyName) == false)
        Path = Application.companyName.RemoveInvalidFileCharacters();

      if (string.IsNullOrEmpty(Application.productName) == false)
        Path += (string.IsNullOrEmpty(Path) == false ? "/" : "") + Application.productName.RemoveInvalidFileCharacters() + "/";
      
      Log.Info($"Using path '{ComposePath("")}'");
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
    /// <param name="slot"></param>
    /// <returns></returns>
    public bool Exists(string filePath)
    {
      Check.IsNotNullOrEmpty(filePath);
      bool success = false;

      try
      {
        if (new FileInfo(ComposePath(filePath)).Exists == true)
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
    /// <param name="data"></param>
    /// <param name="slot"></param>
    /// <typeparam name="T"></typeparam>
    public async void Save<T>(T data, string filePath) where T : LocalFile
    {
      Check.IsNotNull(data);
      Check.IsNotNullOrEmpty(filePath);
      Check.GreaterOrEqual(bufferSize, 4);

      try
      {
        if (CheckPath(filePath) == true)
        {
          filePath = ComposePath(filePath);
          using (FileStream fileStream = new FileStream(filePath,
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
    public async Task<T> Load<T>(string filePath) where T : LocalFile
    {
      Check.IsNotNullOrEmpty(filePath);
      Check.GreaterOrEqual(bufferSize, 4);

      T data = null;

      try
      {
        filePath = ComposePath(filePath);
        if (Exists(filePath) == true)
        {
          using (FileStream sourceStream = new FileStream(filePath,
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
          Log.Error($"File {filePath} not found");
      }
      catch (Exception e)
      {
        Log.Exception(e.ToString());
      }

      return data;
    }

    private string ComposePath(string filePath)
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
      return path + Path + filePath;
    }

    private bool CheckPath(string filePath)
    {
      bool success = false;
      try
      {
        string path = System.IO.Path.GetDirectoryName(ComposePath(filePath));
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
