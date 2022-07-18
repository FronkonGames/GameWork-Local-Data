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
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using FronkonGames.GameWork.Foundation;
using FronkonGames.GameWork.Core;

namespace FronkonGames.GameWork.Modules.LocalData
{
  /// <summary>
  /// Local Data test.
  /// </summary>
  public sealed class LocalDataTest : Game, IGUI
  {
    [Inject]
    private LocalDataModule localData;

    private GUIStyle FontStyle => fontStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 18, richText = true };

    private GUIStyle ButtonStyle => buttonStyle ??= new GUIStyle(GUI.skin.button) { fontSize = 18, richText = true };

    private GUIStyle fontStyle;
    private GUIStyle buttonStyle;

    private Vector2 scrollView;

    private List<FileInfo> files = new List<FileInfo>();
    private int fileSelected = -1;

    /// <summary>
    /// On initialize.
    /// </summary>
    public override void OnInitialize()
    {
    }

    /// <summary>
    /// At the end of initialization.
    /// Called in the first Update frame.
    /// </summary>
    public override void OnInitialized()
    {
      files = localData.Files();
    }

    /// <summary>
    /// OnDrawGizmos event.
    /// </summary>
    public void OnGizmos()
    {
    }

    /// <summary>
    /// OnGUI event.
    /// </summary>
    public void OnGUI()
    {
      const float margin = 10.0f;

      GUILayout.BeginHorizontal(GUILayout.Width(Screen.width - margin * 2));
      {
        GUILayout.Space(margin);

        GUILayout.BeginVertical("box", GUILayout.Width(Screen.width - margin * 2), GUILayout.Height(Screen.height - margin * 2));
        {
          GUILayout.Space(margin);
         
          GUILayout.Label($"Files in '{localData.Path}'", FontStyle);

          GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.Height(Screen.height * 0.3f));
          {
            if (files.Count == 0)
              GUILayout.Label("<i>No files found.</i>", FontStyle);
            else
            {
              scrollView = GUILayout.BeginScrollView(scrollView);
              {
                for (int i = 0; i < files.Count; ++i)
                {
                  GUI.color = i == fileSelected ? Color.cyan : Color.white;
                  
                  GUILayout.BeginHorizontal();
                  {
                    if (GUILayout.Button($"[{i:000}] '{files[i].Name}' | {((int) files[i].Length).BytesToHumanReadable()} | {files[i].CreationTimeUtc.ToShortTimeString()} {files[i].CreationTimeUtc.ToShortDateString()}", FontStyle) == true)
                      fileSelected = i;
                  }
                  GUILayout.EndHorizontal();
                }

                GUI.color = Color.white;
              }
              GUILayout.EndScrollView();
            }
          }
          GUILayout.EndVertical();
          
          GUILayout.BeginHorizontal();
          {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", ButtonStyle) == true)
              files = localData.Files();

            if (GUILayout.Button("New small file", ButtonStyle) == true)
              localData.Save(new FileTest(Rand.Range(1, 10)), localData.NextAvailableName("File_.test"), null, () => { files = localData.Files(); });

            if (GUILayout.Button("New large file", ButtonStyle) == true)
              localData.Save(new FileTest(Rand.Range(10000000, 100000000)), localData.NextAvailableName("File_.test"), null, () => { files = localData.Files(); });

            GUI.enabled = fileSelected != -1;

            if (GUILayout.Button(GUI.enabled == true ? "<color=red>Delete</color>" : "<color=gray>Delete</color>", ButtonStyle) == true && fileSelected < files.Count)
            {
              localData.Delete(files[fileSelected].Name);
              fileSelected = -1;
              
              files = localData.Files();
            }

            GUI.enabled = true;
          }
          GUILayout.EndHorizontal();
          
          GUILayout.Space(margin);
          
          if (fileSelected != -1)
          {
            GUILayout.Label($"File '{files[fileSelected].Name}'", FontStyle);
              
            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
              GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
          }
        }
        GUILayout.EndVertical();
      }
      GUILayout.EndHorizontal();
    }
  }
}
