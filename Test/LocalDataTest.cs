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
using System.IO;
using System.Linq;
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

    private GUIStyle BoxStyle => boxStyle ??= new GUIStyle(GUI.skin.box) { normal = { background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.15f, 0.75f)) } };

    private GUIStyle FontStyle => fontStyle ??= new GUIStyle(GUI.skin.label) { fontSize = 18, richText = true };

    private GUIStyle ButtonStyle => buttonStyle ??= new GUIStyle(GUI.skin.button)
    {
      fontSize = 18,
      richText = true,
      normal = { background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.75f)) },
      hover = { background = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 0.75f)) }
    };

    private GUIStyle boxStyle;
    private GUIStyle fontStyle;
    private GUIStyle buttonStyle;

    private Vector2 scrollView;
    private string opertionsLabel = "NO ACTIVE FILE OPERATIONS";

    private int kilobytes = 10;
    private int megabytes = 0;

    private List<FileInfo> files = new List<FileInfo>();
    private int fileSelected = -1;
    private TestData testData = null;

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
      files = localData.GetFilesInfo();
    }

    /// <summary>
    /// OnDrawGizmos event.
    /// </summary>
    public void OnGizmos()
    {
    }

    public void OnGUI()
    {
      const float margin = 10.0f;

      GUILayout.BeginHorizontal(GUILayout.Width(Screen.width - margin * 2));
      {
        GUILayout.Space(margin);

        GUILayout.BeginVertical(BoxStyle, GUILayout.Width(Screen.width - margin * 2), GUILayout.Height(Screen.height - margin * 2));
        {
          GUILayout.Space(margin);

          GUILayout.BeginHorizontal(GUILayout.Height(Screen.height * 0.4f));
          {
            GUILayout.Space(margin);
            
            GUILayout.BeginVertical(BoxStyle, GUILayout.ExpandWidth(true));
            {
              scrollView = GUILayout.BeginScrollView(scrollView);
              {
                for (int i = 0; i < files.Count; ++i)
                {
                  GUI.color = i == fileSelected ? Color.cyan : Color.white;
                  
                  GUILayout.BeginHorizontal();
                  {
                    string fileLabel = $"'{files[i].Name}'";
                    fileLabel += $" {((int)files[i].Length).BytesToHumanReadable()}";
                    fileLabel += $" {files[i].LastWriteTimeUtc.ToShortTimeString()}";
                    fileLabel += $" {files[i].LastWriteTimeUtc.ToShortDateString()}";

                    if (GUILayout.Button(fileLabel, FontStyle) == true)
                    {
                      fileSelected = i;
                      testData = null;

                      localData.Read<TestData>(files[i].Name,
                        progress => opertionsLabel = $"READING {(progress * 100.0f):00}",
                        file =>
                        {
                          opertionsLabel = "NO ACTIVE FILE OPERATIONS";
                          testData = file;
                        });
                    }
                  }
                  GUILayout.EndHorizontal();
                }

                GUI.color = Color.white;
              }
              GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            GUILayout.Space(margin);

            GUILayout.BeginVertical(BoxStyle, GUILayout.Width(Screen.width * 0.33f));
            {
              GUILayout.BeginHorizontal();
              {
                GUILayout.FlexibleSpace();
                GUILayout.Label(opertionsLabel, fontStyle);
                GUILayout.FlexibleSpace();
              }
              GUILayout.EndHorizontal();

              GUILayout.Space(margin);

              GUI.enabled = !localData.Busy;

              if (GUILayout.Button("OPEN FOLDER", ButtonStyle) == true)
                System.Diagnostics.Process.Start("explorer.exe", $"/select,{localData.Path.Replace("/", "\\")}");

              if (GUILayout.Button("REFRESH", ButtonStyle) == true)
                files = localData.GetFilesInfo();

              GUILayout.Space(margin);

              GUILayout.BeginHorizontal();
              {
                GUILayout.Label("Kilobytes", GUILayout.Width(75.0f));
                kilobytes = (int)GUILayout.HorizontalSlider(kilobytes, 1.0f, 1024);
              }
              GUILayout.EndHorizontal();

              GUILayout.BeginHorizontal();
              {
                GUILayout.Label("Megabytes", GUILayout.Width(75.0f));
                megabytes = (int)GUILayout.HorizontalSlider(megabytes, 0.0f, 98, GUILayout.ExpandWidth(true));
              }
              GUILayout.EndHorizontal();

              if (GUILayout.Button($"CREATE {(megabytes * 1024 * 1024 + kilobytes * 1024).BytesToHumanReadable()} FILE", ButtonStyle) == true)
              {
                localData.CancelAsyncOperations();
                localData.Write(new TestData(megabytes * 1024 * 1024 + kilobytes * 1024),
                  localData.NextAvailableName("File_.test"),
                  null,
                  (file) => { files = localData.GetFilesInfo(); });
              }
              
              GUILayout.Space(margin);

              GUI.enabled = fileSelected != -1 && localData.Busy == false;
            
              if (GUILayout.Button("DELETE", ButtonStyle) == true && fileSelected < files.Count)
              {
                localData.CancelAsyncOperations();
                localData.Delete(files[fileSelected].Name);
                fileSelected = -1;
              
                files = localData.GetFilesInfo();
              }

              GUI.enabled = localData.Busy;
              
              if (GUILayout.Button("CANCEL", ButtonStyle) == true)
              {
              }
             
              GUI.enabled = true;
            }
            GUILayout.EndVertical();
            
            GUILayout.Space(margin);
          }
          GUILayout.EndHorizontal();

          GUILayout.Space(margin);
          
          GUILayout.BeginHorizontal();
          {
            GUILayout.Space(margin);

            GUILayout.BeginVertical(BoxStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
              if (fileSelected != -1 && testData != null)
              {
                GUILayout.Label($"File '{files[fileSelected].Name}'", FontStyle);
              
                GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                {
                  GUILayout.Label($"Message: {testData.message}");
                  GUILayout.Label($"Data size: {testData.data.Length.BytesToHumanReadable()}");
                  
                  string hex = BitConverter.ToString(testData.data[..Math.Min(testData.data.Length, 600)]).Replace("-","");
                  GUILayout.TextArea(testData.data.Length <= 600 ? hex : hex + "...");
                }
                GUILayout.EndVertical();
              }
              
              GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();

            GUILayout.Space(margin);
          }
          GUILayout.EndHorizontal();
          
          GUILayout.Space(margin);
        }
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        
        GUILayout.Space(margin);
      }
      GUILayout.EndHorizontal();
    }
    
    private Texture2D MakeTex(int width, int height, Color col)
    {
      Color[] pix = new Color[width * height];
      for (int i = 0; i < pix.Length; ++i)
        pix[i] = col;

      Texture2D result = new Texture2D(width, height);
      result.SetPixels(pix);
      result.Apply();

      return result;
    }
  }
}
