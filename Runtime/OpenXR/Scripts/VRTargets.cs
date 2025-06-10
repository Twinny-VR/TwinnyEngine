using System;
using UnityEngine;

namespace Twinny.XR
{
    /// <summary>
    /// Holds references to VR tracking targets used in the scene, including hands and head.
    /// </summary>
    public class VRTargets : MonoBehaviour
    {
        #region SERIALIZED_FIELDS
        [Tooltip("References to tracked hand and head targets.")]
        public Targets targets;

        #endregion

        #region NESTED_CLASSES

        /// <summary>
        /// Group of tracking targets for VR interactions.
        /// </summary>
        [Serializable]
        public class Targets
        {
            [Tooltip("Reference to the left OVRHand.")]
            public OVRHand leftHandOVR;

            [Tooltip("Reference to the right OVRHand.")]
            public OVRHand rightHandOVR;

            [Tooltip("Transform reference to the head.")]
            public Transform head;

            [Tooltip("Transform reference to the left hand.")]
            public Transform leftHand;

            [Tooltip("Transform reference to the left index finger.")]
            public Transform leftHandIndex;

            [Tooltip("Transform reference to the left middle finger.")]
            public Transform leftHandMiddle;

            [Tooltip("Transform reference to the left ring finger.")]
            public Transform leftHandRing;

            [Tooltip("Transform reference to the left pinky finger.")]
            public Transform leftHandPink;

            [Tooltip("Transform reference to the left thumb.")]
            public Transform leftHandThumb;

            [Tooltip("Transform reference to the right hand.")]
            public Transform rightHand;

            [Tooltip("Transform reference to the right index finger.")]
            public Transform rightHandIndex;

            [Tooltip("Transform reference to the right middle finger.")]
            public Transform rightHandMiddle;

            [Tooltip("Transform reference to the right ring finger.")]
            public Transform rightHandRing;

            [Tooltip("Transform reference to the right pinky finger.")]
            public Transform rightHandPink;

            [Tooltip("Transform reference to the right thumb.")]
            public Transform rightHandThumb;
        }

        #endregion
    }
}
