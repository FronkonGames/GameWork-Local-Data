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
using System.Collections;
using UnityEngine.TestTools;
using FronkonGames.GameWork.Modules.LocalData;
using UnityEngine;

/// <summary>
/// Local Data tests.
/// </summary>
public partial class LocalDataTests
{
  /// <summary>
  /// Integrity test.
  /// </summary>
  [UnityTest]
  public IEnumerator Integrity()
  {
    GameObject gameObject = new();
    LocalDataModule localDataModule = gameObject.AddComponent<LocalDataModule>();
    localDataModule.OnInitialize();
    localDataModule.OnInitialized();
    
    yield return WriteTest(localDataModule, FileIntegrity.MD5, FileCompression.None, FileEncryption.None);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);

    yield return WriteTest(localDataModule, FileIntegrity.SHA1, FileCompression.None, FileEncryption.None);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);

    yield return WriteTest(localDataModule, FileIntegrity.SHA256, FileCompression.None, FileEncryption.None);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);

    yield return WriteTest(localDataModule, FileIntegrity.SHA512, FileCompression.None, FileEncryption.None);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);

    localDataModule.OnDeinitialize();
    GameObject.DestroyImmediate(gameObject);

    yield return null;
  }
}
