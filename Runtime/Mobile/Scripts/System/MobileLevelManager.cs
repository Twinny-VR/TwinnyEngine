using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Concept.Core;
using Concept.Helpers;
using Twinny.System.Cameras;
using Twinny.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace Twinny.System
{
    using static TwinnyManager;


    public class MobileLevelManager : LevelManager
    {

        protected static new MobileRuntime m_config => TwinnyRuntime.GetInstance<MobileRuntime>();


        public static InterestItem currentInterest;

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

            var envenSystem = EventSystem.current;
            envenSystem.enabled = false;
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnStartLoadScene());
            await CanvasTransition.FadeScreen(true, m_config.fadeTime);



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

            await Task.Delay(1500);
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnLoadScene());
            await CanvasTransition.FadeScreen(false, m_config.fadeTime);
            envenSystem.enabled = true;


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
            Debug.LogWarning("SET FPS");
            await CanvasTransition.FadeScreen(true, m_config.fadeTime);

            if (FirstPersonAgent.isActive)
                CallbackHub.CallAction<ICameraCallBacks>(callback => callback.OnChangeCamera(currentInterest.virtualCamera));

            FirstPersonAgent.TakeControl(!FirstPersonAgent.isActive);

            
            await CanvasTransition.FadeScreen(false, m_config.fadeTime);


        }


    }



}
