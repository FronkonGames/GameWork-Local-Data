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
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using FronkonGames.GameWork.Foundation;
using UnityEngine;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// .
  /// </summary>
  public sealed class LocalDataHandler : IDataHandler
  {
    private readonly string gamePath;

    private const int BufferSize = 1024 * 4;

    public LocalDataHandler()
    {
      if (string.IsNullOrEmpty(Application.companyName) == false)
        gamePath = Application.companyName.RemoveInvalidFileCharacters();

      if (string.IsNullOrEmpty(Application.productName) == false)
        gamePath = (string.IsNullOrEmpty(gamePath) == false ? "/" : "") + Application.productName.RemoveInvalidFileCharacters() + "/";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public bool Exists(int slot)
    {
      Check.IsWithin(slot, 0, 9999);
      bool success = false;

      try
      {
        if (new FileInfo(ComposePath(slot)).Exists == true)
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
    public async void Save<T>(T data, int slot) where T : LocalFile
    {
      Check.IsNotNull(data);
      Check.IsWithin(slot, 0, 9999);

      try
      {
        if (CheckPath(slot) == true)
        {
          string filePath = ComposePath(slot);
          using (FileStream fileStream = new FileStream(filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            BufferSize,
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
    public async Task<T> Load<T>(int slot) where T : LocalFile
    {
      Check.IsWithin(slot, 0, 9999);

      T data = null;

      try
      {
        string filePath = ComposePath(slot);
        if (Exists(slot) == true)
        {
          using (FileStream sourceStream = new FileStream(filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            true))
          {
            byte[] buffer = new byte[BufferSize];

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
    
    private string ComposePath(int slot)
    {
      Check.IsWithin(slot, 0, 9999);

      string path = string.Empty;
#if (UNITY_ANDROID || UNITY_IOS)
      path = $"{Application.persistentDataPath}/";
#elif UNITY_STANDALONE_OSX
      Assert.Fail(@"Not implemented.");
#elif UNITY_STANDALONE_WIN
      path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/My Games/";
#endif

      if (string.IsNullOrEmpty(gamePath) == false)
        path += gamePath;
      path += $"{slot.ToString():0000}.data";
      
      return path;
    }

    private bool CheckPath(int slot)
    {
      Check.IsWithin(slot, 0, 9999);
      
      bool success = false;
      try
      {
        string path = Path.GetDirectoryName(ComposePath(slot));
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
