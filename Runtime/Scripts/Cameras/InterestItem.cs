using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinny.System.Cameras
{
    public class InterestItem : MonoBehaviour
    {

        [SerializeField]
        private State _type = State.LOCKED;
        public State type { get => _type; }


        public float desiredFov = 75f;

        public Vector2 yawRange = new Vector2(0, 240);
        [Range(1, 10)]
        public float yaySpeedMultiply = 1f;

        [Range(0, 1)]
        public float zoomSensitivity = .5f;
        [Range(1,10)]
        public float zoomSpeedMultiply = 1f;
        public float zoomMin = 0f;
        public float zoomMax = 10f;

    }
}
