using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Twinny.UI;
using UnityEngine;

namespace Twinny
{
    [Serializable]
    public class CameraRuntime : ScriptableObject
    {
        [Header("General Camera Config")]
        [VectorLabels("Horizontal", "Vertical", "Zoom")] public Vector3 sesitivity = new Vector3(.1f, .1f, 1f);
        [SerializeField] public bool allowStandby = true;
        [SerializeField] public float standbyInactiveTime = 5f;
        [SerializeField] public float standbyRecenterTime = 2f;
        [SerializeField] public float standbyPanoramicDuration = 60f;
        [SerializeField] public float standbyLockedDuration = 30f;
        [SerializeField] public float standbyRotationSpeed = 1f;

        
        [Header("FPS Camera Config")]
        [SerializeField] public float fpsDesiredFov = 75f;

        [VectorLabels("Min","Max","Radius")] public Vector3 fpsYawRange = new Vector3(100f,600f,600f);
        [SerializeField] public float fpsYawSpeedMultiply = 1f;
        [SerializeField] public float navigationSpeed = 10f;
        [SerializeField] public float navigationangularSpeed = 20f;
        [SerializeField] public float navigationAcceleration = 100f;
        [SerializeField] public float navigationMaxDistance = 20f;
        [SerializeField] public GameObject hitPointPrefab;

        [Header("FPS Camera Blend")]
        public CinemachineBlendDefinition fpsCameraBlend = new CinemachineBlendDefinition() {Style = CinemachineBlendDefinition.Styles.EaseInOut, Time = 2f };


        [Header("Third Camera Config")]
        [SerializeField] public float thirdDesiredFov = 75f;

        [VectorLabels("Min", "Max", "Radius")] public Vector3 thirdYawRange = new Vector3(0f, 240f, -240f);
        [SerializeField] public float thirdYawSpeedMultiply = 1f;
        [VectorLabels("Min", "Max")] public Vector2 thirdZoomRange = new Vector2(120f,240f);
        [SerializeField] public float thirdZoomSpeedMultiply = 1f;

        [Header("Third Camera Blend")]
        public CinemachineBlendDefinition thirdCameraBlend = new CinemachineBlendDefinition() { Style = CinemachineBlendDefinition.Styles.EaseInOut, Time = 2f };


        [Header("Locked Camera Config")]
        [SerializeField] public float lockedDesiredFov = 75f;

        [VectorLabels("Min", "Max", "Radius")] public Vector3 lockedYawRange = new Vector3(0f, 240f, -240f);
        [SerializeField] public float lockedYawSpeedMultiply = 1f;
        [VectorLabels("Min", "Max")][SerializeField] public Vector2 lockedZoomRange = new Vector2(0f,120f);
        [SerializeField] public float lockedZoomSpeedMultiply = 1f;

        [Header("Locked Camera Blend")]
        public CinemachineBlendDefinition lockedCameraBlend = new CinemachineBlendDefinition() { Style = CinemachineBlendDefinition.Styles.EaseInOut, Time = 2f};


        [Header("Panoramic Camera Config")]
        [SerializeField] public float panDesiredFov = 75f;
        [VectorLabels("Min", "Max", "Radius")] public Vector3 panYawRange = new Vector3(150f, 600f, 300f);
        [SerializeField] public float panYawSpeedMultiply = 1f;
        [VectorLabels("Min", "Max")][SerializeField] public Vector2 panZoomRange = new Vector2(-300f,0f);
        [SerializeField] public float panZoomSpeedMultiply = 1f;

        [Header("Panoramic Camera Blend")]
        public CinemachineBlendDefinition panCameraBlend = new CinemachineBlendDefinition() {Style = CinemachineBlendDefinition.Styles.EaseInOut, Time = 2f };

    }
}

