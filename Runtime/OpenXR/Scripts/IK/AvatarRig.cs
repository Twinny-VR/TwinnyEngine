#if false
using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Twinny.XR
{

    [Serializable]
    public class VRMap
    {
        public Transform VRTarget;
        public Transform rigTarget;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
        public TwoBoneIKConstraint idleRig;

        public void Map()
        {
            rigTarget.position = VRTarget.TransformPoint(positionOffset);
            rigTarget.rotation = VRTarget.rotation * Quaternion.Euler(rotationOffset);
        }
    }


    public abstract class AvatarRig : NetworkBehaviour
    {
        public VRMap head;
        public VRMap rightHand;
        public VRMap leftHand;

    }
}
#endif