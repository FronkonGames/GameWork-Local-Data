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
  /// Bounds serialization surrogate.
  /// </summary>
  public sealed class BoundsSerializationSurrogate : ISerializationSurrogate
  {
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
      Bounds bounds = (Bounds)obj;
      info.AddValue("c", bounds.center);
      info.AddValue("s", bounds.size);
      info.AddValue("x", bounds.extents);
      info.AddValue("i", bounds.min);
      info.AddValue("a", bounds.max);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
      Bounds bounds = (Bounds)obj;
      bounds.center = (Vector3)info.GetValue("c", typeof(Vector3));
      bounds.size = (Vector3)info.GetValue("s", typeof(Vector3));
      bounds.extents = (Vector3)info.GetValue("x", typeof(Vector3));
      bounds.min = (Vector3)info.GetValue("i", typeof(Vector3));
      bounds.max = (Vector3)info.GetValue("a", typeof(Vector3));

      return bounds;
    }    
  }
}