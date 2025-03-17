using System;
using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.System;
using Twinny.System.Cameras;
using Twinny.UI;
using UnityEngine;

public class HUDManager : MonoBehaviour, IUICallBacks
{

    #region Fields

    [SerializeField] private GameObject _loadingScreen;
    [Header("SYSTEM CONTROLS FIELDS:")]
    [SerializeField] private GameObject _systemControlsPanel;
    [Header("DEFAULT CONTROLS FIELDS:")]
    [SerializeField] private GameObject _defaultControlsPanel;
    [SerializeField] private GameObject _buttonFPS;
    [SerializeField] private GameObject _buttonHome;
    [SerializeField] private GameObject _buttonPanoramic;
    [Header("SCENE CONTROLS FIELDS:")]
    [SerializeField] private GameObject _sceneControlsPanel;

    #endregion

    #region Properties
    private Animator _animator;
    #endregion

    #region MonoBehaviour Methods

    // Start is called before the first frame update
    void Start()
    {
        CallBackUI.RegisterCallback(this);
        _animator = GetComponent<Animator>();

        ActionManager.RegisterAction("SetPanoramic", SetPanoramic);
        ActionManager.RegisterAction("SetFPS", SetFPS);
    }

    private void OnDestroy()
    {
        CallBackUI.UnregisterCallback(this);
        ActionManager.RemoveAction("SetPanoramic");
        ActionManager.RemoveAction("SetFPS");

    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion

    #region UI Callback Methods

    public void SetPanoramic()
    {
#if !OCULUS
        CameraManager.OnCameraLocked?.Invoke(null);
#endif
    }

    public void SetFPS()
    {
#if !OCULUS
        CameraManager.SetAgentPosition(null);
        CameraManager.SwitchCameraState(State.FPS);

#endif
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
        if(_animator) _animator.SetBool("retracted", false);
        _loadingScreen.SetActive(false);
    }

    void IUICallBacks.OnLoadSceneFeature()
    {
    }

    void IUICallBacks.OnPlatformInitialize()
    {
    }

    void IUICallBacks.OnStartLoadScene()
    {
        if(_animator) _animator.SetBool("retracted", true);
        _loadingScreen.SetActive(true);
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
        _buttonFPS.SetActive(type != "FPS" && type != "THIRD");
        _buttonPanoramic.SetActive(type == "LOCKED");
        _buttonHome.SetActive(type != "PAN");
        Debug.LogWarning("Type: " + type);

    }

    public void OnCameraLocked(Transform target) {

    
    }

    public void OnStandby(bool status)
    {
        if(_animator) _animator.SetBool("retracted", status);
    }

    #endregion
}
