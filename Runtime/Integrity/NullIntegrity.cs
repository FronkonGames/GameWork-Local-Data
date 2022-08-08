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

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// It does not perform any integrity calculations.
  /// </summary>
  public sealed class NullIntegrity : IIntegrity
  {
    /// <summary>
    /// Calculation of the integrity of a stream.
    /// </summary>
    /// <param name="stream">Memory stream.</param>
    /// <param name="progress">Progress of the calculation, from 0 to 1.</param>
    /// <returns>Empty string.</returns>
    public Task<string> Calculate(MemoryStream stream, Action<float> progress = null)
    {
      progress?.Invoke(0.0f);

      return Task.FromResult(string.Empty); 
    }

    /// <summary>
    /// Checks the integrity of a stream.
    /// </summary>
    /// <param name="stream">Memory stream to check.</param>
    /// <param name="hash">Hash.</param>
    /// <param name="progress">Progress of the calculation, from 0 to 1.</param>
    /// <returns>Always blue, I mean, true.</returns>
    public Task<bool> Check(MemoryStream stream, string hash, Action<float> progress = null)
    {
      progress?.Invoke(0.0f);

      return Task.FromResult(true);
    }
  }
}