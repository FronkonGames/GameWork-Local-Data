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
using System.Reflection;
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
    public bool boolValue = false;

    public byte byteValue = 10;
    public sbyte sbyteValue = 20;

    public char charValue = 'M';

    public string stringValue = "All your base are belong to us";

    public short shortValue = 30;
    public ushort ushortValue = 40;

    public int intValue = 50;
    public uint uintValue = 60;

    public long longValue = 70;
    public ulong ulongValue = 80;

    public float floatValue = Mathf.PI;
    public double doubleValue = Math.PI;
    public decimal decimalValue = decimal.One;

    public DateTime dateTimeValue = DateTime.Now;

    public Vector2 vector2 = Vector2.down;
    public Vector3 vector3 = Vector3.up;
    public Vector4 vector4 = Vector4.zero;
    public Color color = Color.magenta;
  }

  private const string fileName = "test.data";

  private TestLocalFile localFile;
  
  /// <summary>
  /// Write / Read test.
  /// </summary>
  [UnityTest]
  public IEnumerator WriteRead()
  {
    GameObject gameObject = new();
    LocalDataModule localDataModule = gameObject.AddComponent<LocalDataModule>();
    localDataModule.OnInitialize();
    localDataModule.OnInitialized();
    
    // @HACK: Set password and seed.
    FieldInfo[] fields = localDataModule.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
    for (int i = 0; i < fields.Length; ++i)
    {
      if (fields[i].Name == "password")
        fields[i].SetValue(localDataModule, "Abracadabra");
      else if (fields[i].Name == "seed")
        fields[i].SetValue(localDataModule, "HocusPocus");
    }
    
    yield return WriteTest(localDataModule, FileIntegrity.None, FileCompression.None, FileEncryption.None);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);
    
    yield return WriteTest(localDataModule, FileIntegrity.MD5, FileCompression.None, FileEncryption.None);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);

    yield return WriteTest(localDataModule, FileIntegrity.None, FileCompression.Zip, FileEncryption.None);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);

    yield return WriteTest(localDataModule, FileIntegrity.None, FileCompression.None, FileEncryption.AES);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);

    yield return WriteTest(localDataModule, FileIntegrity.MD5, FileCompression.Zip, FileEncryption.AES);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);

    localDataModule.OnDeinitialize();
    GameObject.DestroyImmediate(gameObject);

    yield return null;
  }

  private IEnumerator WriteTest(LocalDataModule localDataModule, FileIntegrity integrity, FileCompression compression, FileEncryption encryption)
  {
    float progress = 0.0f;
    FileResult fileResult = FileResult.Cancelled;

    localDataModule.Integrity = integrity;
    localDataModule.Compression = compression;
    localDataModule.Encryption = encryption;

    Task task = localDataModule.Write(new TestLocalFile(), fileName, (value) => progress = value, (value) => fileResult = value);
    yield return AsIEnumeratorReturnNull(task);

    Assert.IsTrue(localDataModule.Exists(fileName));
    Assert.AreEqual(progress, 1.0f);
    Assert.IsTrue(fileResult == FileResult.Ok);
    
    yield return null;
  }

  private IEnumerator ReadTest(LocalDataModule localDataModule)
  {
    float progress = 0.0f;
    FileResult fileResult = FileResult.Cancelled;
    localFile = null;

    Task task = localDataModule.Read<TestLocalFile>(fileName, (value) => progress = value, (result, value) =>
    {
      fileResult = result;
      localFile = value;
    });
    yield return AsIEnumeratorReturnNull(task);

    Assert.IsTrue(localDataModule.Exists(fileName));
    Assert.AreEqual(progress, 1.0f);
    Assert.IsTrue(fileResult == FileResult.Ok);
    Assert.IsNotNull(localFile);
    
    yield return null;
  }

  private void IntegrityTest()
  {
    Assert.IsNotNull(localFile);
  
    Assert.AreEqual(localFile.boolValue, false);
    Assert.AreEqual(localFile.byteValue, 10);
    Assert.AreEqual(localFile.sbyteValue, 20);
    Assert.AreEqual(localFile.charValue, 'M');
    Assert.AreEqual(localFile.stringValue, "All your base are belong to us");
    Assert.AreEqual(localFile.shortValue, 30);
    Assert.AreEqual(localFile.ushortValue, 40);
    Assert.AreEqual(localFile.intValue, 50);
    Assert.AreEqual(localFile.uintValue, 60);
    Assert.AreEqual(localFile.longValue, 70);
    Assert.AreEqual(localFile.ulongValue, 80);
    Assert.AreEqual(localFile.floatValue, Mathf.PI);
    Assert.AreEqual(localFile.doubleValue, Math.PI);
    Assert.AreEqual(localFile.decimalValue, decimal.One);
    Assert.AreEqual(localFile.dateTimeValue.DayOfWeek, DateTime.Now.DayOfWeek);
    Assert.AreEqual(localFile.vector2, Vector2.down);
    Assert.AreEqual(localFile.vector3, Vector3.up);
    Assert.AreEqual(localFile.vector4, Vector4.zero);
    Assert.AreEqual(localFile.color, Color.magenta);
  }
  
  private void DeleteTest(LocalDataModule localDataModule)
  {
    localDataModule.Delete(fileName);
    Assert.IsFalse(localDataModule.Exists(fileName));
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
