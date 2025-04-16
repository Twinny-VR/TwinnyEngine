using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twinny.System.Cameras;
using Twinny.UI;
using UnityEditor;
using UnityEngine;

namespace Twinny.System
{
    public class LevelManagerMobile : LevelManager
    {

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

        protected override void Awake()
        {
            string fileName = "MultiPlatformRuntimePreset";
            config = Resources.Load<TwinnyRuntime>(fileName);

            if (config == null)
            {
                Debug.LogError($"[LevelManager] Impossible to load '{fileName}'.");
            }
        }


        /// <summary>
        /// This Async Method changes the actual scene.
        /// </summary>
        /// <param name="scene">Scene Name</param>
        /// <param name="landMarkIndex">First LandMark to teleport.</param>

        public override async Task ChangeScene(object scene, int landMarkIndex)
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

    }



}
