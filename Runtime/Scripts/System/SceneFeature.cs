using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.System.Cameras;
using UnityEngine;

namespace Twinny.System
{
    public class SceneFeature : TSingleton<SceneFeature>
    {

#if !OCULUS
        [SerializeField]
        private CameraState _sceneType;
        [SerializeField]
        private BuildingFeature _startLocked;

        [SerializeField] private Transform _fpsStartPos;
 


        public BuildingFeature[] centralBuildings;
#endif


        #region MonoBehaviour Methods

        private void Start()
        {
            Init();
#if !OCULUS
                if(_startLocked && _sceneType == CameraState.LOCKED)
                {
                    CameraManager.OnCameraLocked(_startLocked);
                }
                else
                if(_fpsStartPos && _sceneType == CameraState.FPS)
                {
                    CameraManager.SetFPS(_fpsStartPos);
                }                  
#endif
        }

#endregion

    }
}
