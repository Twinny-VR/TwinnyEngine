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
        [Header("Central Camera Config")]
        [SerializeField] public float zoomLimitMin = 0f;
        [SerializeField] public float zoomLimitMax = 1000f;


    }
}
