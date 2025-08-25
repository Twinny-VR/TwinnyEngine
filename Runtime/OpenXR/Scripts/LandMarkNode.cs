using System;
using System.Collections;
using System.Collections.Generic;
using Twinny.UI;
using UnityEngine;
using UnityEngine.Events;


namespace Twinny.System
{

    [Serializable] public class OnLandMarkSelected : UnityEvent { }
    [Serializable] public class OnLandMarkUnSelected : UnityEvent { }

    [Serializable]
    public class LandMarkNode : MonoBehaviour
    {
        public bool changeParent;
        [ShowIf("changeParent")]
        public Transform newParent;

        [Header("NAVIGATION")]
        public LandMarkNode north;
        public LandMarkNode south;
        public LandMarkNode east;
        public LandMarkNode west;


        [Header("CALLBACK ACTIONS")]
        public OnLandMarkSelected OnLandMarkSelected;
        public OnLandMarkUnSelected OnLandMarkUnselected;
    }

}