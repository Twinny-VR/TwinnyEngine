using System.Collections;
using System.Collections.Generic;
using Twinny.System;
using Twinny.System.Cameras;
using Twinny.UI;
using UnityEngine;

public class HUDManager : MonoBehaviour, IUICallBacks
{

    #region Fields

    [SerializeField] private GameObject _buttonHome;

    #endregion

    #region MonoBehaviour Methods

    // Start is called before the first frame update
    void Start()
    {
        CallBackUI.RegisterCallback(this);
    }

    private void OnDestroy()
    {
        CallBackUI.UnregisterCallback(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion

    #region UI Callback Methods



    public void ChangeScene(string sceneName)
    {
        _ = LevelManager.Instance.ChangeScene(sceneName);
    }

    public void BackToHome()
    {
      CameraHandler.OnCameraLocked?.Invoke(null);
    }

    public void ResetExperience()
    {
        _ = LevelManager.Instance.ResetExperience();
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    #endregion
    #region System Callback Methods

    void IUICallBacks.OnExperienceFinished(bool isRunning)
    {
    }

    void IUICallBacks.OnExperienceReady()
    {
    }

    void IUICallBacks.OnExperienceStarted()
    {
    }

    void IUICallBacks.OnExperienceStarting()
    {
    }

    void IUICallBacks.OnHudStatusChanged(bool status)
    {
    }

    void IUICallBacks.OnLoadExtensionMenu(GameObject menu)
    {
    }

    void IUICallBacks.OnLoadScene()
    {
    }

    void IUICallBacks.OnLoadSceneFeature()
    {
    }

    void IUICallBacks.OnPlatformInitialize()
    {
    }

    void IUICallBacks.OnStartLoadScene()
    {
    }


    void IUICallBacks.OnUnloadSceneFeature()
    {
    }
#if FUSION2

    void IUICallBacks.OnSwitchManager(int source)
    {
    }
#endif

    public void OnCameraChanged(Transform camera, string type)
    {



    }

    public void OnCameraLocked(Transform target) {

        _buttonHome.SetActive(target != null);
    
    }


    #endregion
}
