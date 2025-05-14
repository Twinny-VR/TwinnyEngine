
using System.Collections;
using Concept.Helpers;
using Fusion;
using Twinny.Localization;
using Twinny.System;
using Twinny.System.Network;
using Twinny.XR;
using UnityEngine;

namespace Twinny
{
    public class Recenter : MonoBehaviour
    {

        private bool _restarting;


        // Start is called before the first frame update
        void Start()
        {
            if (OVRManager.display != null)
                OVRManager.display.RecenteredPose += OnRecenterDetected;

            //  StartCoroutine(DisplayCheck());
        }


        private void OnRecenterDetected()
        {
            if (_restarting) return;


            Debug.LogWarning("[LevelManager] Recenter was detected.");
            Debug.LogWarning("OVRManager:" + OVRManager.instance);
            Debug.LogWarning("OVRDisplay:" + OVRManager.display);

            
            if (NetworkRunnerHandler.runner.GameMode != GameMode.Single)
            {

                if (LevelManagerXR.instance == null || (LevelManagerXR.isRunning && (!NetworkRunnerHandler.runner || !NetworkRunnerHandler.runner.IsConnectedToServer)))
                {
                        _restarting = true;
                    Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%DISCONECTED_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Error);

                    Debug.LogWarning("CHAMA RESET");

                    AsyncOperationExtensions.CallDelayedAction(() => {


                       _ = LevelManagerXR.instance.ResetExperience();

                    }, LevelManagerXR.Config.resetExperienceDelay*1000);
                      
                    return;
                }




                AnchorManager.Recolocation();

             //   OVRColocationSession.StartDiscoveryAsync();

            }



        }


        private IEnumerator DisplayCheck()
        {
            while (true)
            {
                OVRSpatialAnchor[] anchors = FindObjectsByType<OVRSpatialAnchor>(FindObjectsSortMode.None);
                Debug.Log("=======================");
                Debug.LogWarning("OVRManager: " + OVRManager.instance);
                Debug.LogWarning("DISPLAY: " + OVRManager.display);
                Debug.LogWarning("AnchorManager: " + AnchorManager.Instance);
                Debug.LogWarning("LevelManager: " + NetworkedLevelManager.Instance);
                Debug.LogWarning("Anchors: " + anchors.Length);
                Debug.Log("=======================");
                yield return new WaitForSeconds(3f);

            }
        }
    }
}
