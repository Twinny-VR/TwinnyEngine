#if !OCULUS
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Twinny.UI;
using UnityEngine;

namespace Twinny.System.Cameras
{
    public class InterestItem : MonoBehaviour
    {
        [Header("Camera Configuration")]

        [SerializeField]
        protected State _type = State.LOCKED;
        public State type { get => _type; }

        public bool overrideCameraSettings;

        [ShowIf("overrideCameraSettings")]
        public float desiredFov = 75f;

        [ShowIf("overrideCameraSettings")]
        public Vector3 yawRange = new Vector3(0, 250,600);

        [ShowIf("overrideCameraSettings")]
        public float yawSpeedMultiply = 1f;

#if UNITY_EDITOR
        [HideInInspector]
        public bool showZoomSettings;
#endif

        [ShowIf("showZoomSettings")]
        public Vector2 zoomRange = new Vector2(0, 100);

        [ShowIf("showZoomSettings")]
        public float zoomSpeedMultiply = 1f;

        public bool overrideCameraBlend;
        [ShowIf("overrideCameraBlend")]
        public CinemachineBlendDefinition cameraBlend = new CinemachineBlendDefinition();


        #region MonoBehaviour Methods
#if UNITY_EDITOR

        protected virtual void OnValidate()
        {
            showZoomSettings = overrideCameraSettings && _type != State.FPS;
        }

#endif
        #endregion

    }
}
#endif