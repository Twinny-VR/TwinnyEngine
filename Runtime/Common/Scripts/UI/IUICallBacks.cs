using Fusion;
using UnityEngine;


namespace Twinny.UI
{
    public interface IUICallBacks
    {
        void OnHudStatusChanged(bool status);

        #region Experience Callbacks

        void OnPlatformInitialize();
        void OnExperienceReady();
        void OnExperienceFinished(bool isRunning);
        void OnLoadExtensionMenu(GameObject menu);
        void OnStartLoadScene();
        void OnLoadScene();
        void OnLoadSceneFeature();
        void OnUnloadSceneFeature();
        void OnExperienceStarting();
        void OnExperienceStarted();

        #endregion

        void OnSwitchManager(int source);

        #region Camera Callbacks

        void OnStandby(bool status);

        void OnCameraChanged(Transform camera, string type);
        void OnCameraLocked(Transform target);
        #endregion



    }
}
