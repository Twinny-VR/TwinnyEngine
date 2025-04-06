using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Twinny.UI;
using Twinny.Helpers;

using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System;
using System.Data.Common;

using Fusion;

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
        /// <param name="sceneIndex">Scene name</param>
        /// <param name="landMarkIndex">LandMark in scene</param>
        /// <returns></returns>
        public static async Task LoadAdditiveSceneAsync(object scene, int landMarkIndex)
        {
             await Task.Delay(500); // Similar "yield return new WaitForSeconds(.5f)"

            if (SceneFeature.Instance)
            {
                await UnloadAdditivesScenes();
            }


            if (NetworkRunnerHandler.Instance)
            {
                if (scene is SceneRef index)
                    await NetworkRunnerHandler.runner.LoadScene(index, LoadSceneMode.Additive);
                else
                    await NetworkRunnerHandler.runner.LoadScene(scene as string, LoadSceneMode.Additive);

                NetworkedLevelManager.Instance.RPC_NavigateTo(landMarkIndex, false);
            }else
            if (scene is string name)
                await AsyncOperationExtensions.WaitForSceneLoadAsync(SceneManager.LoadSceneAsync(name));
            else
                await AsyncOperationExtensions.WaitForSceneLoadAsync(SceneManager.LoadSceneAsync((int)scene));


/* 
            if (scene is string name)
                await AsyncOperationExtensions.WaitForSceneLoadAsync(SceneManager.LoadSceneAsync(name));
            else
                await AsyncOperationExtensions.WaitForSceneLoadAsync(SceneManager.LoadSceneAsync((int)scene));
*/

        }

        public static async Task UnloadAdditivesScenes() {
            await Task.Delay(500); // Similar "yield return new WaitForSeconds(.5f)"

            //Scene mainScene = SceneManager.GetActiveScene();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene =  SceneManager.GetSceneAt(i);
                if (loadedScene.buildIndex > 1)
                {
                    await NetworkRunnerHandler.runner.UnloadScene(loadedScene.name);
//                    await AsyncOperationExtensions.WaitForSceneLoadAsync(SceneManager.UnloadSceneAsync(loadedScene.name));
                }
            }

            
        }


#endregion





    }

}
