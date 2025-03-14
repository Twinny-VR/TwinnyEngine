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
        public BuildingFeature[] centralBuildings;
#endif


        #region MonoBehaviour Methods

        private void Start()
        {
            Init();
#if !OCULUS
       //     CameraHandler.OnCameraLocked?.Invoke(_sensorLocked);
#endif
        }

#endregion

    }
}
