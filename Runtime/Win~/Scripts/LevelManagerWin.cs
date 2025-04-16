using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinny.System
{
    public class LevelManagerWin : LevelManager
    {
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

            await Task.Delay(1500);
            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnLoadScene());
            await CanvasTransition.FadeScreen(false);

        }

        protected override void OnPlatformInitialized(Platform platform)
        {
            if (config.autoStart)
            {
                _ = ChangeScene(2, 0);
            }
            else
                _ = CanvasTransition.FadeScreen(false);
        }


    }
}
