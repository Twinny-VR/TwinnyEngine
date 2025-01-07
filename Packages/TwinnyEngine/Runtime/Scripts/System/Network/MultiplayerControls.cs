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
    [Obsolete("MultiplayerControls is deprecated. Use 'NetworkManager' instead.")]
    public class MultiplayerControls : IControls
{
        #region Fields
        private Scene _currentScene;

        #endregion
        #region Public Methods

        public void SetUp()
        {
            CheckVoipConnectionAsync();
        }

        /// <summary>
        /// Async function to load an additive scene
        /// </summary>
        /// <param name="scene">Scene name</param>
        /// <param name="landMarkIndex">LandMark in scene</param>
        /// <returns></returns>
        public async Task LoadAdditiveSceneAsync(string scene, int landMarkIndex)
        {
            await Task.Delay(500); // Similar "yield return new WaitForSeconds(.5f)"

            if (SceneFeature.Instance)
            {
                await UnloadAdditivesScenes();

                // Unload Current Scene on Async Mode
               // NetworkSceneAsyncOp asyncUnload = LevelManager.runner.UnloadScene(_currentScene.name);
                //await AsyncOperationExtensions.WaitForSceneLoadAsync(asyncUnload);
                //                await SceneManager.UnloadSceneAsync(_currentScene).ToTask();
            }

            // Load New Scene on Async Mode
            NetworkSceneAsyncOp asyncLoad = LevelManager.runner.LoadScene(scene, LoadSceneMode.Additive);

            // Wait scene complete loading
            await AsyncOperationExtensions.WaitForSceneLoadAsync(asyncLoad);


            //SceneFeature.Instance.TeleportToLandMark(landMarkIndex);
            LevelManager.Instance.RPC_NavigateTo(landMarkIndex, false);

        }

        public async Task UnloadAdditivesScenes() {
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

            _currentScene = default;
        }


        /// <summary>
        /// Get primary recorder state
        /// </summary>
        /// <returns>Returns if it's currently transmiting</returns>
        public bool GetVoipStatus()
        {
            /*
            Recorder recorder = PunVoiceClient.Instance?.PrimaryRecorder;
            if (recorder)
                return recorder.IsCurrentlyTransmitting;
            else */return false;
        }

        /// <summary>
        /// Switch primary recorder transmission
        /// </summary>
        public void SetVoip()
        {
            /*
            Recorder recorder = PunVoiceClient.Instance?.PrimaryRecorder;
            if (recorder)
            {
                recorder.TransmitEnabled = !recorder.TransmitEnabled;
            }
            else
                Debug.LogError("Primary recorder not found.");*/
        }

        #endregion

        #region Private Methods
        private async Task CheckVoipConnectionAsync()
        {
            /*
            Recorder recorder = PunVoiceClient.Instance?.PrimaryRecorder;

            while (recorder)
            {
                LevelManager.OnVoipChanged?.Invoke(recorder.IsCurrentlyTransmitting);

                await Task.Delay(3000); 
            }

            Debug.Log("Voice recorder not found.");
            */
        }


        #endregion



    }

}