using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinny.System
{

    [Serializable]
    public class TwinnyRuntime : ScriptableObject
    {
        [SerializeField]
        public float fadeTime = 1f;

        [SerializeField]
        public Material defaultSkybox;



    }
}
