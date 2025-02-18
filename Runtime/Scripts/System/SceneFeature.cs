using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.System.Cameras;
using UnityEngine;

namespace Twinny.System
{
    public class SceneFeature : TSingleton<SceneFeature>
    {

        [SerializeField] private Transform _sensorCenter;
        public Transform sensorCenter {  get { return _sensorCenter; } }

        [SerializeField] private BuildingFeature _sensorLocked;
        public BuildingFeature sensorLocked { get { return _sensorLocked; } }



        #region MonoBehaviour Methods

        private void Start()
        {
            Init();
            CameraHandler.OnCameraLocked?.Invoke(_sensorLocked);
        }

        #endregion

    }
}
