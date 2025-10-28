using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Concept.Addressables;
using Concept.Core;
using Concept.Helpers;
using Twinny.System.Cameras;
using Twinny.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Twinny.System
{
    using static TwinnyManager;


    public class MobileLevelManager : LevelManager
    {

        protected static new MobileRuntime m_config => TwinnyRuntime.GetInstance<MobileRuntime>();

        public static event Action<float> OnCutoffChangedEvent;


        public static InterestItem currentInterest;
        [SerializeField]
        private float m_cutoffHeight = 4.5f;
        public static float cutoffHeight => GetInstance().m_cutoffHeight;

        protected EventSystem m_eventSystem = EventSystem.current;

        #region MonoBehaviour Methods

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (AssetDatabase.IsValidFolder("Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string fileName = "MobileRuntimePreset";
            string assetPath = "Assets/Resources/" + fileName + ".asset";
            MobileRuntime preset = AssetDatabase.LoadAssetAtPath<MobileRuntime>(assetPath);

            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<MobileRuntime>();
                AssetDatabase.CreateAsset(preset, assetPath);
                AssetDatabase.SaveAssets();
                Debug.Log("Novo preset 'MobileRuntimePreset' criado e salvo em: " + assetPath);
            }

        }
#endif

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }
        protected override void Awake()
        {
            base.Awake();
            TwinnyRuntime.GetInstance<MobileRuntime>();
        }


        protected override void Start()
        {
            base.Start();
            ActionManager.RegisterAction("SetFPS", SetFPS);
        }
        #endregion


        public static void ResetCutoff()
        {
            float cutoffHeight = MobileLevelManager.cutoffHeight;
            Shader.SetGlobalFloat("_CutoffHeight", cutoffHeight);
        }

        public static void OnCutoffChanged(float value)
        {
            float height = cutoffHeight * value;
            Shader.SetGlobalFloat("_CutoffHeight", height);
            OnCutoffChangedEvent?.Invoke(value);

        }




        /// <summary>
        /// This Async Method changes the actual scene.
        /// </summary>
        /// <param name="scene">Scene Name</param>
        /// <param name="landMarkIndex">First LandMark to teleport.</param>
        public override async Task ChangeScene(object scene, int landMarkIndex)
        {
            if (IsSceneLoaded(scene)) {
                CallbackHub.CallAction<IUICallBacks>(callback => callback.OnLoadScene());               
                return; 
            }
            if(m_eventSystem == null)
                m_eventSystem = EventSystem.current;
            m_eventSystem.enabled = false;
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnStartLoadScene());
            await CanvasTransition.FadeScreen(true, m_config.fadeTime);
            ResetCutoff();



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

            _ = Resources.UnloadUnusedAssets();

            MobileSceneFeature feature = SceneFeature.Instance as MobileSceneFeature;

            if (feature)
            {

                //TODO Check this implementation
                if (feature.interestPoints.Length > 0)
                {
                    InterestItem interest = feature.interestPoints[landMarkIndex];
                    ChangeInterest(interest);
                }
                else
                    Debug.LogError("[LevelManager] Scenes must at least on InterestItem set in SceneFeature!");

            }

            await Task.Delay(500);
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnLoadScene());
            await CanvasTransition.FadeScreen(false, m_config.fadeTime);
            m_eventSystem.enabled = true;


        }


        private Dictionary<string, SceneInstance> m_loadedAddressableScenes = new Dictionary<string, SceneInstance>();

        public async Task ChangeAddressableScene(string sceneKey, int landMarkIndex = 0)
        {
            if (m_eventSystem == null)
                m_eventSystem = EventSystem.current;
            m_eventSystem.enabled = false;
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnStartLoadScene());
            await CanvasTransition.FadeScreen(true, m_config.fadeTime);
            ResetCutoff();

            await UnloadAddressableScenes();

            await AddressablesManager.LoadAddressableScene(sceneKey, true);

            _ = Resources.UnloadUnusedAssets();

            MobileSceneFeature feature = SceneFeature.Instance as MobileSceneFeature;

            if (feature)
            {

                //TODO Check this implementation
                if (feature.interestPoints.Length > 0)
                {
                    InterestItem interest = feature.interestPoints[landMarkIndex];
                    ChangeInterest(interest);
                }
                else
                    Debug.LogError("[LevelManager] Scenes must at least on InterestItem set in SceneFeature!");

            }

            await Task.Delay(500);
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnLoadScene());
            await CanvasTransition.FadeScreen(false, m_config.fadeTime);
            m_eventSystem.enabled = true;


        }

        public async Task UnloadAddressableScenes()
        {
            await Task.Delay(500);


            // Cria uma cópia da lista para evitar modificar durante iteração
            var scenesToUnload = new List<KeyValuePair<string, SceneInstance>>(m_loadedAddressableScenes);
            m_loadedAddressableScenes.Clear();

            foreach (var scenePair in scenesToUnload)
            {
                string sceneKey = scenePair.Key;
                SceneInstance sceneInstance = scenePair.Value;

                await AddressablesManager.UnloadAddressableScene(sceneInstance, sceneKey);
            }

            Debug.Log("Descarregamento de cenas Addressables concluído");
        }


        public static void ChangeInterest(InterestItem interest)
        {

            if (!interest) Debug.LogError("[MobileLevelManager] Interest Item not found.");

            currentInterest = interest;


            Transform targetTransform = (interest is InterestBuilding building)
                ? building.fpsSpawnPoint
                : interest.transform;


            if (targetTransform)
                FirstPersonAgent.TeleportTo(targetTransform);

            if ((!interest.virtualCamera))
            {
                FirstPersonAgent.TakeControl(interest.transform);
            }
            else
                CallbackHub.CallAction<ICameraCallBacks>(callback => callback.OnChangeCamera(interest.virtualCamera));

            /*
            if (interest is BuildingFeature)
            {
                BuildingFeature building = interest as BuildingFeature;
                if (interest.type == State.LOCKED || interest.type == State.LOCKEDTHIRD)
                {
                    OldCameraManager.OnCameraLocked(building);
                }
                else
                    Debug.LogError("[LevelManager] Wrong building format for Locked scene. Only LOCKED or THIRD are supported.");
            }
            */

        }

        public static MobileLevelManager GetInstance()
        {
            return Instance as MobileLevelManager;
        }

        public void SetFPS()
        {
            _ = SetFPSAsync();
        }

        public async Task SetFPSAsync()
        {
            await CanvasTransition.FadeScreen(true, m_config.fadeTime);

            ResetCutoff();


            if (FirstPersonAgent.isActive)
                CallbackHub.CallAction<ICameraCallBacks>(callback => callback.OnChangeCamera(currentInterest.virtualCamera));

            FirstPersonAgent.TakeControl(!FirstPersonAgent.isActive);

            
            await CanvasTransition.FadeScreen(false, m_config.fadeTime);


        }


    }



}
