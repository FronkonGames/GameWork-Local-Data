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
using System.Collections;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using FronkonGames.GameWork.Modules.LocalData;
using UnityEngine;

/// <summary>
/// Write tests.
/// </summary>
public class LocalDataTests
{
  [Serializable]
  private class TestLocalFile : LocalData
  {
    public bool boolValue;

    public byte byteValue;
    public sbyte sbyteValue;

    public char charValue;

    public string stringValue;

    public short shortValue;
    public ushort ushortValue;

    public int intValue;
    public uint uintValue;

    public long longValue;
    public ulong ulongValue;

    public float floatValue;
    public double doubleValue;
    public decimal decimalValue;

    public DateTime dateTimeValue;

    public Vector2 vector2;
    public Vector3 vector3;
    public Vector4 vector4;
    public Color color;
  }

  private const string fileName = "test.data";
  
  /// <summary>
  /// Write test.
  /// </summary>
  [UnityTest]
  public IEnumerator Write()
  {
    GameObject gameObject = new();
    LocalDataModule localDataModule = gameObject.AddComponent<LocalDataModule>();
    localDataModule.OnInitialize();
    localDataModule.OnInitialized();

    yield return WriteTest(localDataModule, FileIntegrity.None, FileCompression.None, FileEncryption.None);
    yield return WriteTest(localDataModule, FileIntegrity.MD5, FileCompression.None, FileEncryption.None);
    yield return WriteTest(localDataModule, FileIntegrity.None, FileCompression.Zip, FileEncryption.None);
    yield return WriteTest(localDataModule, FileIntegrity.None, FileCompression.None, FileEncryption.AES);
    yield return WriteTest(localDataModule, FileIntegrity.MD5, FileCompression.Zip, FileEncryption.AES);

    localDataModule.OnDeinitialize();
    GameObject.DestroyImmediate(gameObject);

    yield return null;
  }

  private IEnumerator WriteTest(LocalDataModule localDataModule, FileIntegrity integrity, FileCompression compression, FileEncryption encryption)
  {
    float progress = 0.0f;
    FileResult result = FileResult.Cancelled;

    Task task = localDataModule.Write(new TestLocalFile(), fileName, (value) => progress = value, (value) => result = value);
    yield return AsIEnumeratorReturnNull(task);

    Assert.IsTrue(localDataModule.Exists(fileName));
    Assert.AreEqual(progress, 1.0f);
    Assert.IsTrue(result == FileResult.Ok);

    localDataModule.Delete(fileName);
    Assert.IsFalse(localDataModule.Exists(fileName));
    
    yield return null;
  }

  private static IEnumerator AsIEnumeratorReturnNull(Task task)
  {
    while (task.IsCompleted == false)
      yield return null;
 
    if (task.IsFaulted == true)
      ExceptionDispatchInfo.Capture(task.Exception).Throw();
 
    yield return null;
  }  
}
