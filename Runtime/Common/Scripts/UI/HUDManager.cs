using System;
using System.Collections.Generic;
using System.Linq;
using Concept.Core;
using Concept.Helpers;
using Twinny.System;
using UnityEngine;


namespace Twinny.UI {
    public abstract class HUDManager : TSingleton<HUDManager>, IUICallBacks
    {

        #region Fields

        [Header("SYSTEM CONTROLS FIELDS:")]
        [SerializeField] protected GameObject _loadingScreen;


        [SerializeField] protected UIElement[] _uIElements;

        [SerializeField] protected UICallBackEvents _uICallBacks;
        public IUICallBacks CallBacks => _uICallBacks;

        #endregion

        #region Properties
        protected Animator _animator;
        #endregion

        #region MonoBehaviour Methods

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            foreach (var item in _uIElements)
            {
                if (item.element && string.IsNullOrEmpty(item.key)) item.key = item.element.name;
            }
        }
#endif

        // Start is called before the first frame update

         protected override void Start()
        {
            base.Start();
            CallbackHub.RegisterCallback(this);
            _animator = GetComponent<Animator>();
            _uIElements.RegisterElements();

        }

        protected virtual void OnDestroy()
        {
            CallbackHub.UnregisterCallback(this);
            _uIElements.UnregisterElements();
        }

        #endregion


        public virtual void ShowControlsMenu(bool status)
        {
            if (_animator) _animator.SetBool("retracted", !status);

        }

        #region System Callback Methods

        public virtual void OnExperienceFinished(bool isRunning) { }

        public virtual void OnExperienceReady() { }

        public virtual void OnExperienceStarted() { }

        public virtual void OnExperienceStarting() { }

        public virtual void OnHudStatusChanged(bool status) { }

        public virtual void OnLoadExtensionMenu(GameObject menu, bool isStatic) { }

        public virtual void OnLoadScene()
        {
            _loadingScreen.SetActive(false);
        }

        public virtual void OnLoadSceneFeature() { }

        public virtual void OnPlatformInitialize() { }

        public virtual void OnStartLoadScene()
        {
            ShowControlsMenu(false);

            _loadingScreen.SetActive(true);
        }


        public virtual void OnUnloadSceneFeature()
        {
        }

        public virtual void OnSwitchManager(int source)
        {
        }

        public virtual void OnCameraChanged(Transform camera, string type)
        {
            Debug.LogWarning("Type: " + type);

        }

        public virtual void OnCameraLocked(Transform target)
        {


        }

        public virtual void OnStandby(bool status)
        {
                        ShowControlsMenu(status);

        }

        public void OnPlayerList(int count)
        {
        }
        #endregion
    }
    [Serializable]
    public class UIElement
    {
        public string key;
        public GameObject element;
    }

    [Serializable]
    public static class UIElementsProvider
    {
        private static List<UIElement> _elements = new List<UIElement>();
        public static List<UIElement> elements { get => _elements; }

        public static void RegisterElements(this UIElement[] uIElements) {
            foreach (var item in uIElements)
            {
                RegisterElement(item);
            }
        }
        public static void RegisterElement(UIElement uIElement) {

            if (_elements.Contains(uIElement))
            {
                Debug.LogWarning($"[UIElementsProvider] Trying to insert a duplicate '{uIElement.key}' element.");
                return;
            }

            _elements.Add(uIElement);
        }
        public static void UnregisterElements(this UIElement[] uIElements) {
            foreach (var item in uIElements)
            {
                UnregisterElement(item);
            }

        }
        public static void UnregisterElement(UIElement uIElement) {
            if (_elements.Contains(uIElement))
            {
            _elements.Remove(uIElement);
                return;
            }
                Debug.LogWarning($"[UIElementsProvider] Failed to remove: '{uIElement.key}' element not found.");
        }

        public static GameObject GetElement(string key)
        {
            return _elements.FirstOrDefault(element => element.key == key)?.element;
        }

        public static void ShowElement(string key)
        {
            GetElement(key)?.SetActive(true);
        }

        public static void HideElement(string key)
        {
            GetElement(key)?.SetActive(false);
        }


    }

}
