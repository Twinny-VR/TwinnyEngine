using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Twinny.UI;
using Twinny.Helpers;

using Photon.Voice.Unity;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using Fusion;
using System;

namespace Twinny.System.Network
{
    /// <summary>
    /// This class controls Multiplayer Network Level
    /// </summary>
    public static class NetworkSceneManager 
{
        #region Public Methods
        /// <summary>
        /// Async function to load an additive scene
        /// </summary>
        /// <param name="scene">Scene name</param>
        /// <param name="landMarkIndex">LandMark in scene</param>
        /// <returns></returns>
        public static async Task LoadAdditiveSceneAsync(string scene, int landMarkIndex)
        {
             await Task.Delay(500); // Similar "yield return new WaitForSeconds(.5f)"

            if (SceneFeature.Instance)
            {
                await UnloadAdditivesScenes();

            }

            await LevelManager.runner.LoadScene(scene, LoadSceneMode.Additive);


            LevelManager.Instance.RPC_NavigateTo(landMarkIndex, false);

        }

        public static async Task UnloadAdditivesScenes() {
            await Task.Delay(500); // Similar "yield return new WaitForSeconds(.5f)"

            Scene mainScene = SceneManager.GetActiveScene();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene =  SceneManager.GetSceneAt(i);
                if (loadedScene != mainScene)
                {
                    await LevelManager.runner.UnloadScene(loadedScene.name);
                }
            }

            
        }


        #endregion





    }

}