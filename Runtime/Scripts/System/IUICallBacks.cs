#if FUSION2
using Fusion;
#endif
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

#if FUSION2
        void OnSwitchManager(int source);
#endif

#region Camera Callbacks

        void OnCameraChanged(Transform camera, string type);
        void OnCameraLocked(Transform target);
#endregion


    }
}
