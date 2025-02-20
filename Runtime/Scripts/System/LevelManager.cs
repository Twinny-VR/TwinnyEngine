using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twinny.Helpers;
using Twinny.Localization;
using Twinny.System.Cameras;
using Twinny.System.Network;
using Twinny.UI;
using Twinny.XR;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Twinny.System
{

    public class LevelManager : TSingleton<LevelManager>
    {

        [SerializeField] public MultiPlatformRuntime _config;
        public static MultiPlatformRuntime Config { get { return Instance._config; } }




        #region MonoBehaviour Methods


#if UNITY_EDITOR
        private void OnValidate()
        {
                if (AssetDatabase.IsValidFolder("Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                string fileName = "MultiPlatformRuntimePreset.asset";
                string assetPath = "Assets/Resources/" + fileName;
                MultiPlatformRuntime preset = AssetDatabase.LoadAssetAtPath<MultiPlatformRuntime>(assetPath);

                if (preset == null)
                {
                    preset = ScriptableObject.CreateInstance<MultiPlatformRuntime>();
                    AssetDatabase.CreateAsset(preset, assetPath);
                    AssetDatabase.SaveAssets();
                    Debug.Log("Novo preset 'MultiPlatformRuntimePreset' criado e salvo em: " + assetPath);
                }

                _config = AssetDatabase.LoadAssetAtPath<MultiPlatformRuntime>(assetPath);



        }
#endif

        void Awake()
        {

            _config = Resources.Load<MultiPlatformRuntime>("MultiPlatformRuntimePreset");

            if (_config == null)
            {
                Debug.LogError("[LevelManager] Impossible to load 'MultiPlatformRuntimePreset'.");
            }

        }


        // Start is called before the first frame update
        void Start()
        {
            Init();
            TwinnyManager.OnPlatformInitialize += OnPlatformInitialized;
            _ = TwinnyManager.InitializePlatform();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            TwinnyManager.OnPlatformInitialize -= OnPlatformInitialized;

        }

        #endregion

        #region Public Methods


        public async Task ResetExperience()
        {
            CallBackUI.CallAction(callback => callback.OnExperienceFinished(false));

            await CanvasTransition.FadeScreen(true);

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// This Async Method changes the actual scene.
        /// </summary>
        /// <param name="scene">Scene Name</param>
        /// <param name="landMarkIndex">First LandMark to teleport.</param>
        public async Task ChangeScene(object scene)
        {

            CallBackUI.CallAction(callback => callback.OnStartLoadScene());


            await CanvasTransition.FadeScreen(true);
#if !OCULUS
            CameraHandler.OnCameraLocked?.Invoke(null);
#endif
            //TODO Mudar o sistema de carregamento de cenas
            if (scene is string name && name == "PlatformScene")
            {
                await UnloadAdditivesScenes();
                CallBackUI.CallAction(callback => callback.OnExperienceFinished(true));
            }
            else
            {
                await LoadAdditiveSceneAsync(scene);

            }

            await CanvasTransition.FadeScreen(false);

        }


        /// <summary>
        /// Async function to load an additive scene
        /// </summary>
        /// <param name="sceneIndex">Scene name</param>
        /// <param name="landMarkIndex">LandMark in scene</param>
        /// <returns></returns>
        public static async Task LoadAdditiveSceneAsync(object scene)
        {
            await Task.Delay(500); // Similar "yield return new WaitForSeconds(.5f)"

                await UnloadAdditivesScenes();


            if (scene is string name)
                await AsyncOperationExtensions.WaitForSceneLoadAsync(SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive));
            else
                await AsyncOperationExtensions.WaitForSceneLoadAsync(SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Additive));
        }

        public static async Task UnloadAdditivesScenes()
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


#endregion


        #region CallBack Methods

        private void OnPlatformInitialized(Platform platform)
        {
            if (_config.autoStart)
            {
                _ = ChangeScene(2);
            }   else         
            _ = CanvasTransition.FadeScreen(false);
        }
        #endregion

    }

}