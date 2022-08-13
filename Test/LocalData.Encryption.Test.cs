﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
  /// Encryption test.
  /// </summary>
  [UnityTest]
  public IEnumerator Encryption()
  {
    GameObject gameObject = new();
    LocalDataModule localDataModule = gameObject.AddComponent<LocalDataModule>();
    localDataModule.OnInitialize();
    localDataModule.OnInitialized();
    
    SetPassword(localDataModule, "0123456789012345", "0123456789012345");

    yield return WriteTest(localDataModule, FileIntegrity.None, FileCompression.None, FileEncryption.AES);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);

    yield return WriteTest(localDataModule, FileIntegrity.None, FileCompression.None, FileEncryption.TripleDES);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);

    yield return WriteTest(localDataModule, FileIntegrity.None, FileCompression.None, FileEncryption.RC2);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);

    SetPassword(localDataModule, "01234567", "01234567890");

    yield return WriteTest(localDataModule, FileIntegrity.None, FileCompression.None, FileEncryption.DES);
    yield return ReadTest(localDataModule);
    IntegrityTest();
    DeleteTest(localDataModule);

    localDataModule.OnDeinitialize();
    GameObject.DestroyImmediate(gameObject);

    yield return null;
  }
}