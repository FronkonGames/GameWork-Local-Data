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
  /// Rect serialization surrogate.
  /// </summary>
  public sealed class RectSerializationSurrogate : ISerializationSurrogate
  {
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
      Rect rect = (Rect)obj;
      info.AddValue("x", rect.x);
      info.AddValue("y", rect.y);
      info.AddValue("w", rect.width);
      info.AddValue("h", rect.height);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
      Rect rect = (Rect)obj;
      rect.x = info.GetSingle("x");
      rect.y = info.GetSingle("y");
      rect.width = info.GetSingle("w");
      rect.height = info.GetSingle("h");

      return rect;
    }    
  }
}