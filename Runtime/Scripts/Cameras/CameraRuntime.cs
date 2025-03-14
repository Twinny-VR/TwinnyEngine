using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinny
{
    [Serializable]
    public class CameraRuntime : ScriptableObject
    {
        [Header("General Camera Config")]
        [SerializeField] public float horizontalSensitivity = .1f;
        [SerializeField] public float verticalSensitivity = .1f;
        [SerializeField] public float zoomSensitivity = .3f;
        [SerializeField] public bool allowStandby = true;
        [SerializeField] public float standbyInactiveTime = 5f;
        [SerializeField] public float standbyRecenterTime = 2f;
        [SerializeField] public float standbyPanoramicDuration = 60f;
        [SerializeField] public float standbyLockedDuration = 30f;
        [SerializeField] public float standbyRotationSpeed = 1f;

        [SerializeField] public float navigationSpeed = 100f;
        [SerializeField] public float navigationangularSpeed = 120f;
        [SerializeField] public float navigationAcceleration = 30f;
        [SerializeField] public float navigationDistanceMax = 20f;
        [SerializeField] public GameObject hitPointPrefab;

        [Header("Third Camera Config")]
        [SerializeField] public float thirdZoomLimitMin = 20f;
        [SerializeField] public float thirdZoomLimitMax = 0f;

    }
}
