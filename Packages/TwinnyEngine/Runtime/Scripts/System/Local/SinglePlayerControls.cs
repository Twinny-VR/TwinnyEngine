using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.System.Local;
using Twinny.System.Network;
using Twinny.UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;


namespace Twinny.System.Local
{

    /// <summary>
    /// This class controls Local Network Level
    /// </summary>
    [Obsolete("SinglePlayerControls is deprecated. Use 'NetworkManager' instead.")]
    public class SinglePlayerControls : IControls
    {

        #region Fields
        private Scene _currentScene;
        #endregion

        public void SetUp() { }


        /// <summary>
        /// Async function to load an additive scene
        /// </summary>
        /// <param name="scene">Scene name</param>
        /// <param name="landMarkIndex">LandMark in scene</param>
        /// <returns></returns>
        public async Task LoadAdditiveSceneAsync(string scene, int landMarkIndex)
        {
            await Task.Delay(500); // Similar "yield return new WaitForSeconds(.5f)"

            if (_currentScene.IsValid() || _currentScene != default)
            {
                // Unload Current Scene on Async Mode
                await SceneManager.UnloadSceneAsync(_currentScene).ToTask();
            }

            // Load New Scene on Async Mode
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

            // Wait scene complete loading
           await asyncLoad.ToTask();

            _currentScene = SceneManager.GetSceneByName(scene);

            SceneFeature.Instance.transform.position = AnchorManager.Instance.transform.position;
            SceneFeature.Instance.transform.rotation = AnchorManager.Instance.transform.rotation;
            SceneFeature.Instance.gameObject.AddComponent<OVRSpatialAnchor>();    
            SceneFeature.Instance.TeleportToLandMark(landMarkIndex);
            HUDManager.Instance.FadeScreen(true);
        }


        /// <summary>
        /// Async function to unload all aditives scene loaded
        /// </summary>
        /// <returns></returns>
        public async Task UnloadAdditivesScenes() {

            await Task.Delay(500); // Similar "yield return new WaitForSeconds(.5f)"

            Scene mainScene = SceneManager.GetActiveScene();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene != mainScene)
                {
                    await SceneManager.UnloadSceneAsync(loadedScene).ToTask();
                }
            }
            _currentScene = default;
        }

    }
    
}




