#if OCULUS && FUSION2

using System.Collections;
using System.Threading.Tasks;
using Fusion;
using Twinny.Localization;
using Twinny.System;
using Twinny.System.Network;
using Twinny.XR;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                    Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%DISCONECTED_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Error);
                    _restarting = true;
                    Task.Run(async () =>
                    {
                        await Task.Delay(5000);
                        SceneManager.LoadScene(0);//StartScene always must be 0
                    });

                    return;
                }




                AnchorManager.Recolocation();

                OVRColocationSession.StartDiscoveryAsync();

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
                Debug.LogWarning("LevelManager: " + NetworkedLevelManager.instance);
                Debug.LogWarning("Anchors: " + anchors.Length);
                Debug.Log("=======================");
                yield return new WaitForSeconds(3f);

            }
        }
    }
}
#endif