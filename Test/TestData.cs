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
using FronkonGames.GameWork.Foundation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FronkonGames.GameWork.Modules.LocalData
{
  [Serializable]
  public struct StructData
  {
    public string stringValue;
  }

  [Serializable]
  public class ClassData
  {
    public string stringValue;
  }

  /// <summary>
  /// Test file.
  /// </summary>
  [Serializable]
  public sealed class TestData : LocalData
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

    public StructData structData;
    public ClassData classData;

    public List<int> listValue;
    public Dictionary<int, string> dictValue;

    public Vector2 vector2;
    public Vector3 vector3;
    public Vector4 vector4;
    public Color color;

    public byte[] data;

    public TestData(int size, float randomness = 0.5f)
    {
      Check.Greater(size, 0);

      boolValue = true;
      byteValue = 42;
      sbyteValue = -42;
      charValue = 'A';
      stringValue = "All your base are belong to us!";
      shortValue = -42;
      ushortValue = 42;
      intValue = -42;
      uintValue = 42;
      longValue = -42;
      ulongValue = 42;
      floatValue = Mathf.PI;
      doubleValue = Math.PI;
      decimalValue = (decimal)Math.E;
      dateTimeValue = DateTime.Now;

      structData = new StructData { stringValue = "This is a structure." };
      classData = new ClassData { stringValue = "This is a class." };

      listValue = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
      dictValue = new Dictionary<int, string>
      {
        { 0, "Zero" },
        { 1, "One" },
        { 2, "Two" },
        { 3, "Three" },
        { 4, "Four" },
        { 5, "Five" },
        { 6, "Six" },
        { 7, "Seven" },
        { 8, "Eight" },
        { 9, "Nine" },
      };

      vector2 = Vector2.zero;
      vector3 = Vector3.forward;
      vector4 = Vector4.one;
      color = Color.magenta;
      
      data = new byte[size];
      bool random = false;
      for (int i = 0; i < size; ++i)
      {
        data[i] = random ? (byte)Random.Range(0, 255) : (byte)(i % 255);

        if (i % 1024 == 0)
          random = Random.value < randomness;
      }
    }
  }
}
