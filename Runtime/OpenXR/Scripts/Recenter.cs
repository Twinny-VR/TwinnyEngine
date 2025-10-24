
using System.Collections;
using Concept.Helpers;
using Fusion;
using Meta.XR.MultiplayerBlocks.Fusion;
using Twinny.System;
using Twinny.System.Network;
using Twinny.XR;
using UnityEngine;

namespace Twinny
{
    public class Recenter : MonoBehaviour
    {

        private bool _restarting;
        private bool _lostTracking;
        public bool colocationReady;
        [SerializeField] private FusionNetworkBootstrapper _colocationBootstrapper;
        // Start is called before the first frame update
        void Start()
        {
            if (OVRManager.display != null)
            {
                OVRManager.TrackingAcquired += OnTrackingAcquired;
                OVRManager.InputFocusAcquired += OnInputFocusAcquired;
                OVRManager.display.RecenteredPose += OnRecenterDetected;
            }

            //  StartCoroutine(DisplayCheck());
        }

        private void OnDestroy()
        {
            if (OVRManager.display != null)
            {
                OVRManager.TrackingAcquired -= OnTrackingAcquired;
                OVRManager.InputFocusAcquired -= OnInputFocusAcquired;
                OVRManager.display.RecenteredPose -= OnRecenterDetected;
            }
        }

        private void Update()
        {

            /*
            if (!OVRManager.isHmdPresent || !OVRManager.tracker.isPositionTracked)
            {
                // tracking perdido ou HMD removido
                if (!_lostTracking)
                {
                    _lostTracking = true;
                }
            }
            else if (_lostTracking && LevelManagerXR.isRunning)
            {
                _lostTracking = false;
                return;
                     Debug.Log("Tracking perdido. Reiniciando a experiencia!!!");
                AsyncOperationExtensions.CallDelayedAction(() => {


                    _ = LevelManagerXR.instance.ResetExperience();

                }, (int)(LevelManagerXR.Config.resetExperienceDelay*1000));
            }
            */
        }

        private void OnTrackingAcquired()
        {
            if (LevelManagerXR.isRunning)
            {
                Debug.LogWarning("Tracking perdido. Reiniciando a experiencia!!!");
                AsyncOperationExtensions.CallDelayedAction(() =>
                {


                    _ = LevelManagerXR.instance.ResetExperience();

                }, (int)(TwinnyRuntime.GetInstance<RuntimeXR>().resetExperienceDelay * 1000));
            }
        }

        private void OnInputFocusAcquired()
        {
            Debug.LogWarning($"INPUT FOCUS AQUIRED!! {LevelManagerXR.isRunning}");
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
                    //Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%DISCONECTED_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Error);

                    Debug.LogWarning("%DISCONECTED_MESSAGE");
                       _ = LevelManagerXR.instance.ResetExperience();
                      
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
