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
using UnityEngine;
using FronkonGames.GameWork.Foundation;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// Small file.
  /// </summary>
  [Serializable]
  public sealed class FileTest : LocalFile
  {
    public byte valueByte;
    public char valueChar;
    public string valueString;
    public int valueInt;
    public long valueLong;
    public float valueFloat;
    public double valueDouble;

    public List<int> listInts;

    public FileTest(int size)
    {
      Check.Greater(size, 0);

      valueByte = (byte)Rand.Range(0, 255);
      valueChar = (char)Rand.Range(0, 255);
      valueString = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..8];
      valueInt = Rand.Range(0, 255);
      valueLong = Rand.Range(0, 255);
      valueFloat = Rand.Range(0.0f, 255.0f);
      valueDouble = (double)Rand.Range(0.0f, 255.0f);

      listInts = new List<int>(size);
      for (int i = 0; i < listInts.Count; ++i)
        listInts[i] = Rand.Range(0, 255);
    }
  }
}
