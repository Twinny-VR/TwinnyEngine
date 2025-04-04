using System;
using System.Collections;
using System.Collections.Generic;
using Twinny.System;
using Twinny.System.Network;
using UnityEngine;


namespace Twinny.XR {

    [Serializable]
#if NETWORK
public class RuntimeXR : NetworkRuntime
#else
public class RuntimeXR : TwinnyRuntime
#endif
    {
        [SerializeField]
        public bool allowClickSafeAreaOutside = false;
        [SerializeField]
        public Vector2 safeAreaSize = new Vector2(2.5f, 1.5f);

    }
}
