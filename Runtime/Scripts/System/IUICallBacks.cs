#if FUSION2
using Fusion;
#endif
using UnityEngine;

namespace Twinny.UI
{
    public interface IUICallBacks
    {
        void OnHideHud(bool status);

        void OnPlatformInitialize();
        void OnExperienceReady();
        void OnExperienceFinished(bool isRunning);
        void OnLoadExtensionMenu(GameObject menu);
        void OnStartLoadScene();
        void OnLoadScene();
        void OnLoadSceneFeature();
        void OnUnloadSceneFeature();
#if FUSION2
        void OnExperienceStarting(PlayerRef source);
        void OnExperienceStarted(PlayerRef source);
        void OnSwitchManager(PlayerRef source);
#endif
    }
}
