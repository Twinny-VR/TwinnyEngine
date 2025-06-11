using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinny.System
{

    [Serializable]
    public class TwinnyRuntime : ScriptableObject
    {
        public bool isTestBuild = true;
        public float fadeTime = 1f;
        public Material defaultSkybox;



    }
}
