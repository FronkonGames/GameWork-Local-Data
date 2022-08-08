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
using System.Runtime.Serialization;
using UnityEngine;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// Matrix4x4 serialization surrogate.
  /// </summary>
  public sealed class Matrix4x4SerializationSurrogate : ISerializationSurrogate
  {
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
      Matrix4x4 matrix4X4 = (Matrix4x4)obj;
      info.AddValue("00", matrix4X4.m00);
      info.AddValue("10", matrix4X4.m10);
      info.AddValue("20", matrix4X4.m20);
      info.AddValue("30", matrix4X4.m30);
      info.AddValue("01", matrix4X4.m01);
      info.AddValue("11", matrix4X4.m11);
      info.AddValue("21", matrix4X4.m21);
      info.AddValue("31", matrix4X4.m31);
      info.AddValue("02", matrix4X4.m02);
      info.AddValue("12", matrix4X4.m12);
      info.AddValue("22", matrix4X4.m22);
      info.AddValue("32", matrix4X4.m32);
      info.AddValue("03", matrix4X4.m03);
      info.AddValue("13", matrix4X4.m13);
      info.AddValue("23", matrix4X4.m23);
      info.AddValue("33", matrix4X4.m33);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
      Matrix4x4 matrix4X4 = (Matrix4x4)obj;
      matrix4X4.m00 = info.GetSingle("00");
      matrix4X4.m10 = info.GetSingle("10");
      matrix4X4.m20 = info.GetSingle("20");
      matrix4X4.m30 = info.GetSingle("30");
      matrix4X4.m01 = info.GetSingle("01");
      matrix4X4.m11 = info.GetSingle("11");
      matrix4X4.m21 = info.GetSingle("21");
      matrix4X4.m31 = info.GetSingle("31");
      matrix4X4.m02 = info.GetSingle("02");
      matrix4X4.m12 = info.GetSingle("12");
      matrix4X4.m22 = info.GetSingle("22");
      matrix4X4.m32 = info.GetSingle("32");
      matrix4X4.m03 = info.GetSingle("03");
      matrix4X4.m13 = info.GetSingle("13");
      matrix4X4.m23 = info.GetSingle("23");
      matrix4X4.m33 = info.GetSingle("33");

      return matrix4X4;
    }    
  }
}