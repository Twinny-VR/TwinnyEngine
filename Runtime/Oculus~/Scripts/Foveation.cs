using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Twinny.System.Cameras
{
    public class Foveation : MonoBehaviour
    {
        private XRDisplaySubsystem _xrDisplaySubsystem;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float _strength = 1.0f;

        void Start()
        {

            // Find the XR display subsystem
            var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems<XRDisplaySubsystem>(xrDisplaySubsystems);

            if (xrDisplaySubsystems.Count < 1)
            {
                Debug.LogWarning("[Foveation] No XR Display Subsystems found.");
                return;
            }
            foreach (var subsystem in xrDisplaySubsystems)
            {
                if (subsystem.running)
                {
                    _xrDisplaySubsystem = subsystem;
                    break;
                }
            }
            _xrDisplaySubsystem.foveatedRenderingFlags = XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
            SetFRLevel();
        }

        public void SetFRLevel()
        {
            _xrDisplaySubsystem.foveatedRenderingLevel = _strength;
        }
    }
}