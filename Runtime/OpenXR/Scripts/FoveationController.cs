using System.Collections.Generic;
using Concept.Helpers;
using UnityEngine;
using UnityEngine.XR;

namespace Twinny.XR
{

public class FoveationController : MonoBehaviour
{
    void Start()
    {
        AsyncOperationExtensions.CallDelayedAction(FoveationInitialize, 1000);
    }

    void FoveationInitialize() {

        Debug.Log("[FoveationController] Foveated Render initializing...");
        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(displaySubsystems);

        foreach (var subsystem in displaySubsystems)
        {
            if (subsystem.running)
            {
                subsystem.foveatedRenderingLevel = 1;
                subsystem.foveatedRenderingFlags = XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
                Debug.Log("[FoveationController] Foveated Rendering config successfully!");
                return;
            }
        }

        Debug.LogWarning($"[FoveationController] Impossible to config the Foveated Rendering: {(displaySubsystems.Count == 0 ? "No Display Subsystem found" : "None Display Subsystem running")}.");

      //  OVRManager.foveatedRenderingLevel = OVRManager.FoveatedRenderingLevel.High;
        OVRManager.eyeTrackedFoveatedRenderingEnabled = true;
    }
}

}