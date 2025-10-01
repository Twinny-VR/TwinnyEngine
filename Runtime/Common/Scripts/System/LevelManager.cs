using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Concept.Core;
using Concept.Helpers;
using Twinny.Helpers;
using Twinny.Localization;
using Twinny.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Twinny.System
{

    public abstract class LevelManager : TSingleton<LevelManager>
    {
        protected static TwinnyRuntime m_config => TwinnyRuntime.GetInstance<TwinnyRuntime>();
        #region Fields

        public delegate void onExperienceFinished();
        public static onExperienceFinished OnExperienceFinished;

        #endregion



        #region MonoBehaviour Methods




        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            //OnPlatformInitialize += OnPlatformInitialized;
            //_ = InitializePlatform();
        }

        private void OnDestroy()
        {
            //OnPlatformInitialize -= OnPlatformInitialized;

        }


        #endregion

        #region Public Methods

        public static void StartExperience(string scene, int landMarkIndex) { }
        public static void QuitExperience() { }
        public virtual void GetReady() { }

        public virtual async Task ResetExperience()
        {
            OnExperienceFinished?.Invoke();
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnExperienceFinished(false));

            await CanvasTransition.FadeScreen(true, m_config.fadeTime);

            SceneManager.LoadScene(0);
        }


        public virtual void ResetApplication() { }

        /// <summary>
        /// This Async Method changes the actual scene.
        /// </summary>
        /// <param name="scene">Scene Name</param>
        /// <param name="landMarkIndex">First LandMark to teleport.</param>
        public virtual async Task ChangeScene(object scene, int landMarkIndex)
        {

            await CanvasTransition.FadeScreen(true, m_config.fadeTime);

            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnStartLoadScene());


            //TODO Mudar o sistema de carregamento de cenas
            if (scene is string name && name == "PlatformScene")
            {
                await UnloadAdditivesScenes();
                CallbackHub.CallAction<IUICallBacks>(callback => callback.OnExperienceFinished(true));
            }
            else
            {
                await LoadAdditiveSceneAsync(scene);

            }
            /*
            SceneFeature feature = SceneFeature.Instance as SceneFeature;

            if (feature)
            {

                if (feature.interestPoints.Length > 0)
                {
                    InterestItem interest = feature.interestPoints[landMarkIndex];
                    if (interest is BuildingFeature)
                    {
                        BuildingFeature building = interest as BuildingFeature;
                        if (interest.type == State.LOCKED || interest.type == State.LOCKEDTHIRD)
                        {
                            CameraManager.OnCameraLocked(building);
                        }
                        else
                            Debug.LogError("[LevelManager] Wrong building format for Locked scene. Only LOCKED or THIRD are supported.");
                    }
                    else
                    {
                        CameraManager.SwitchCamera(interest);
                    }
                }
                else
                    Debug.LogError("[LevelManager] Locked Scenes must at least on centralBuilding set in SceneFeature!");

            }
            */
            await Task.Delay(1500);
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnLoadScene());
            await CanvasTransition.FadeScreen(false, m_config.fadeTime);

        }


        /// <summary>
        /// Async function to load an additive scene
        /// </summary>
        /// <param name="sceneIndex">Scene name</param>
        /// <param name="landMarkIndex">LandMark in scene</param>
        /// <returns></returns>
        public virtual async Task LoadAdditiveSceneAsync(object scene)
        {
            await Task.Delay(500); // Similar "yield return new WaitForSeconds(.5f)"

            await UnloadAdditivesScenes();
            if (scene is string name)
                await AsyncOperationExtensions.WaitForSceneLoadAsync(SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive));
            else
                await AsyncOperationExtensions.WaitForSceneLoadAsync(SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Additive));
        }

        public virtual async Task UnloadAdditivesScenes()
        {
            await Task.Delay(500); // Similar "yield return new WaitForSeconds(.5f)"

            //Scene mainScene = SceneManager.GetActiveScene();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.buildIndex > 1)
                {
                    await AsyncOperationExtensions.WaitForSceneLoadAsync(SceneManager.UnloadSceneAsync(loadedScene.name));
                }
            }


        }

        /// <summary>
        /// This method is only for handle the Fade Screen during the teleporting
        /// </summary>
        /// <param name="landMarkIndex">LandMark to Teleport</param>
        public static async void NavigateTo(int landMarkIndex)
        {
            await CanvasTransition.FadeScreen(true, m_config.fadeTime);
            SceneFeature.Instance.TeleportToLandMark(landMarkIndex);
            await CanvasTransition.FadeScreen(false, m_config.fadeTime);
        }


        public static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.name == sceneName)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsSceneLoaded(int buildIndex)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.buildIndex == buildIndex)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsSceneLoaded(object scene)
        {
            if (scene is string name)
                return IsSceneLoaded(name);
            else
            if (scene is int index)
                return IsSceneLoaded(index);

            return false;
        }

        #endregion


        #region CallBack Methods

        protected virtual void OnPlatformInitialized(Platform platform)
        {
                _ = CanvasTransition.FadeScreen(false, m_config.fadeTime);
        }
        #endregion

    }

}