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
//#define ENABLE_PROFILING
using System.Collections.Generic;
using UnityEngine;
using FronkonGames.GameWork.Core;
using FronkonGames.GameWork.Foundation;

namespace FronkonGames.GameWork.Modules.DataPersistence
{
  /// <summary>
  /// .
  /// </summary>
  public sealed class DataPersistenceModule : MonoBehaviourModule,
                                              IInitializable,
                                              ISceneLoad
  {
    /// <summary>
    /// Is it initialized?
    /// </summary>
    /// <value>Value</value>
    public bool Initialized { get; set; }

    [SerializeField, String("game.data", "Data file name.")]
    private string fileName;

    [SerializeField, Bool(false, "Use compressed data.")]
    private bool compress;

    [SerializeField, Bool(false, "Use encrypted data.")]
    private bool encrypted;

    private IDataHandler dataHandler;

    private List<IDataPersistence> dataPersistenceObjects = new List<IDataPersistence>();

    /// <summary>
    /// When initialize.
    /// </summary>
    public void OnInitialize()
    {
      dataHandler = new LocalDataHandler();
    }

    /// <summary>
    /// At the end of initialization.
    /// Called in the first Update frame.
    /// </summary>
    public void OnInitialized()
    {
    }

    /// <summary>
    /// When deinitialize.
    /// </summary>
    public void OnDeinitialize()
    {
    }

    /// <summary>
    /// Scene is loaded.
    /// </summary>
    public void OnSceneLoad(int sceneBuildIndex)
    {
      UpdatePersistenceDataObjects();

      LoadPersistenceData();
    }

    /// <summary>
    /// Scene is unloaded.
    /// </summary>
    public void OnSceneUnload() => SavePersistenceData();

    private void UpdatePersistenceDataObjects()
    {
#if ENABLE_PROFILING
      using (Profiling.Time("Find persistence data"))
#endif
      {
        dataPersistenceObjects.Clear();

        MonoBehaviour[] monoBehaviours = FindObjectsOfType<MonoBehaviour>();
        for (int i = 0; i < monoBehaviours.Length; ++i)
        {
          if (monoBehaviours[i] is IDataPersistence)
            dataPersistenceObjects.Add(monoBehaviours[i] as IDataPersistence);
        }
      }
    }

    private void SavePersistenceData()
    {
#if ENABLE_PROFILING
      using (Profiling.Time("Save persistence data"))
#endif
      {
      }
    }

    private void LoadPersistenceData()
    {
#if ENABLE_PROFILING
      using (Profiling.Time("Load persistence data"))
#endif
      {
      }
    }
  }
}
