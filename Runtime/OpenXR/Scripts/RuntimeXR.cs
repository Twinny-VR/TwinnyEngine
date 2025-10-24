using System;
using System.Collections;
using System.Collections.Generic;
using Twinny.System;
using Twinny.System.Network;
using UnityEngine;


namespace Twinny.XR {

    [Serializable]
    [CreateAssetMenu(fileName = "RuntimeXRPreset", menuName = "Twinny/System/RuntimeXR Preset", order = 1)]

    public class RuntimeXR : NetworkRuntime
    {
        [SerializeField]
        public bool allowClickSafeAreaOutside = false;
        [SerializeField]
        public Vector2 safeAreaSize = new Vector2(2.5f, 1.5f);


    }
}
