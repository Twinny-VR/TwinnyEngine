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
    public class MobileLevelManager : LevelManager
    {

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

            TwinnyManager.LoadRuntimeProfile<MobileRuntime>(fileName);

        }
#endif

        protected override void Awake()
        {
            Init();
            string fileName = "MobileRuntimePreset";
            TwinnyManager.LoadRuntimeProfile<MobileRuntime>(fileName);
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
            await CanvasTransition.FadeScreen(true);

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
            await CanvasTransition.FadeScreen(false);
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

        public void SetFPS()
        {
            if (FirstPersonAgent.isActive)
                CallbackHub.CallAction<ICameraCallBacks>(callback => callback.OnChangeCamera(currentInterest.virtualCamera));

            FirstPersonAgent.TakeControl(!FirstPersonAgent.isActive);

        }


    }



}
