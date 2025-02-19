using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.System.Cameras;
using UnityEngine;

namespace Twinny.System
{
    public class SceneFeature : TSingleton<SceneFeature>
    {

        [SerializeField] private Transform _sensorFollow;
        public Transform sensorFollow {  get { return _sensorFollow; } }
        [SerializeField] private Transform _sensorLook;
        public Transform sensorLook {  get { return _sensorLook; } }
#if !OCULUS
        [SerializeField] private BuildingFeature _sensorLocked;
        public BuildingFeature sensorLocked { get { return _sensorLocked; } }
#endif


        #region MonoBehaviour Methods

        private void Start()
        {
            Init();
#if !OCULUS
            CameraHandler.OnCameraLocked?.Invoke(_sensorLocked);
#endif
        }

#endregion

    }
}
