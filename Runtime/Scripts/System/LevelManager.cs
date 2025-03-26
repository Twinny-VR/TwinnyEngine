using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twinny.Helpers;
using Twinny.Localization;
using Twinny.System.Cameras;
using Twinny.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Twinny.System
{

    public class LevelManager : TSingleton<LevelManager>
    {
        #region Fields

        [SerializeField] public TwinnyRuntime config;
        public static MultiPlatformRuntime Config { get { return Instance.config as MultiPlatformRuntime; } }


        public delegate void onExperienceFinished();
        public static onExperienceFinished OnExperienceFinished;

        #endregion

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

            config = AssetDatabase.LoadAssetAtPath<MultiPlatformRuntime>(assetPath);



        }
#endif

        void Awake()
        {

            config = Resources.Load<MultiPlatformRuntime>("MultiPlatformRuntimePreset");

            if (config == null)
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
            OnExperienceFinished?.Invoke();
            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnExperienceFinished(false));

            await CanvasTransition.FadeScreen(true);

            SceneManager.LoadScene(0);
        }

        /// <summary>
        /// This Async Method changes the actual scene.
        /// </summary>
        /// <param name="scene">Scene Name</param>
        /// <param name="landMarkIndex">First LandMark to teleport.</param>
        public async Task ChangeScene(object scene, int landMarkIndex)
        {

            await CanvasTransition.FadeScreen(true);

            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnStartLoadScene());


            //TODO Mudar o sistema de carregamento de cenas
            if (scene is string name && name == "PlatformScene")
            {
                await UnloadAdditivesScenes();
                CallBackUI.CallAction<IUICallBacks>(callback => callback.OnExperienceFinished(true));
            }
            else
            {
                await LoadAdditiveSceneAsync(scene);

            }

            SceneFeatureMulti feature = SceneFeature.Instance as SceneFeatureMulti;

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

            await Task.Delay(1500);
            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnLoadScene());
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

        /// <summary>
        /// This method is only for handle the Fade Screen during the teleporting
        /// </summary>
        /// <param name="landMarkIndex">LandMark to Teleport</param>
        public async void NavigateTo(int landMarkIndex)
        {
            await CanvasTransition.FadeScreen(true);
            SceneFeatureMulti.Instance.TeleportToLandMark(landMarkIndex);
            await CanvasTransition.FadeScreen(false);
        }


        #endregion


        #region CallBack Methods

        private void OnPlatformInitialized(Platform platform)
        {
            if (Config.autoStart)
            {
                _ = ChangeScene(2, 0);
            }
            else
                _ = CanvasTransition.FadeScreen(false);
        }
        #endregion

    }

}