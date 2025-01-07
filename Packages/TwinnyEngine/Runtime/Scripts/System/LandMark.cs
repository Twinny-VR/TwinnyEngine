using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Twinny.System
{
    [Serializable]
    public class LandMark
    {
        [HideInInspector] public string landName;
        [SerializeField] public LandMarkNode node;
        [SerializeField] public Material skyBoxMaterial;
        [Range(0f, 360f)]
        [SerializeField] public float hdriOffsetRotation;
    }
}