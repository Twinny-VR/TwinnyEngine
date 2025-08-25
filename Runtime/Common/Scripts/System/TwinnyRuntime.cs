using System;
using System.Collections;
using System.Collections.Generic;
using Concept.UI;
using UnityEngine;

namespace Twinny.System
{

    [Serializable]
    public class TwinnyRuntime : ScriptableObject
    {
        public bool isTestBuild = true;
        public float fadeTime = 1f;
        public Material defaultSkybox;
        public bool forceFrameRate;
        [ShowIf("forceFrameRate")]
        [Range(60,120)]
        public int targetFrameRate = 90;



    }
}
