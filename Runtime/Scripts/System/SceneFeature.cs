using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.System.Cameras;
using Twinny.UI;
using UnityEngine;

namespace Twinny.System
{
    public class SceneFeature : TSingleton<SceneFeature>
    {

#if !OCULUS
        [SerializeField]
        private State _sceneType;
        public State sceneType { get => _sceneType; }


#if UNITY_EDITOR
        [HideInInspector] public bool showStartPos;
        [ShowIf("showStartPos")]
#endif
        [SerializeField] private Transform _fpsStartPos;
        public Transform fpsStartPos { get => _fpsStartPos; }

#if UNITY_EDITOR
        [HideInInspector] public bool showLocked;
        [ShowIf("showLocked")]
#endif
        public BuildingFeature[] centralBuildings;

#endif


        #region MonoBehaviour Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
            showLocked = _sceneType == State.PAN || _sceneType == State.LOCKED;
            showStartPos = _sceneType == State.FPS;
        }
#endif
        private void Start()
        {
            Init();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method change World Transform position to a especific landMark position.
        /// </summary>
        /// <param name="landMarkIndex">Index on landMarks array.</param>
        public void TeleportToLandMark(int landMarkIndex)
        {
            if (centralBuildings.Length > 0)
            {
                BuildingFeature building = centralBuildings[landMarkIndex];
                
                CameraManager.OnCameraLocked(building);
            }
        }

        #endregion
    }
}
