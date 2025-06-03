using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Twinny.System
{

    [Serializable] public class OnLandMarkSelected : UnityEvent { }
    [Serializable] public class OnLandMarkUnSelected : UnityEvent { }

    public class LandMarkNode : MonoBehaviour
{

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