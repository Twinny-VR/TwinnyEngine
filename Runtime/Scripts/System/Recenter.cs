using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Twinny.System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PlasticGui.PlasticTableColumn;

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
        }


        private void OnRecenterDetected()
        {
            if (_restarting) return;


            Debug.LogWarning("[LevelManager] Recenter was detected.");
            Debug.LogWarning("OVRManager:" + OVRManager.instance);
            Debug.LogWarning("OVRDisplay:" + OVRManager.display);

            if (LevelManager.Instance == null ||  (LevelManager.Instance.isRunning && (!LevelManager.runner || !LevelManager.runner.IsConnectedToServer)))
            {
                Debug.LogWarning("[DISCONNECTED] Application is restarting.");
                _restarting = true;
                Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    SceneManager.LoadScene(0);//StartScene always must be 0
                });

                return;
            }

            if (LevelManager.runner.GameMode != GameMode.Single)
            {


                AnchorManager.Recolocation();

                OVRColocationSession.StartDiscoveryAsync();

            }




        }

    }
}
