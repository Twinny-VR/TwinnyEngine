using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Twinny.System
{


public class LandMarkNode : MonoBehaviour
{
    [Header("NAVIGATION")]
    public LandMarkNode north;
    public LandMarkNode south;
    public LandMarkNode east;
    public LandMarkNode west;

}

}