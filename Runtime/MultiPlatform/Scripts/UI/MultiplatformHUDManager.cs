using Twinny.Helpers;
using UnityEngine;

namespace Twinny.UI
{
    public class MultiplatformHUDManager : HUDManager
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            ActionManager.RegisterAction("SetPanoramic", SetPanoramic);
            ActionManager.RegisterAction("SetFPS", SetFPS);
        }

        // Update is called once per frame
        void Update()
        {
        
        }



    #region UI Callback Methods

    public void SetPanoramic()
        {
            CameraManager.OnCameraLocked?.Invoke(null);
        }

        public void SetFPS()
        {
            CameraManager.Instance.fpsAgent.TeleportTo(null);
            CameraManager.SwitchCamera(null);
        }


        #endregion


    }
}
