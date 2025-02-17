using System;
using System.Collections;
using System.Collections.Generic;
using Twinny.System;
using Twinny.System.Network;
using UnityEngine;


namespace Twinny.XR {

    [Serializable]
public class RuntimeXR : NetworkRuntime
    {
        [SerializeField]
        public bool allowClickSafeAreaOutside = false;

    }
}