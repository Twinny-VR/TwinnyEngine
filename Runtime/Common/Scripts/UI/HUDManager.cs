using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Twinny.Helpers;
using Twinny.System;
using Twinny.UI;
using UnityEngine;


namespace Twinny.UI {
    public abstract class HUDManager : MonoBehaviour, IUICallBacks
    {

        #region Fields

        [Header("SYSTEM CONTROLS FIELDS:")]
        [SerializeField] private GameObject _loadingScreen;


        [SerializeField] private UIElement[] _uIElements;

        [SerializeField] private UICallBackEvents _uICallBacks;
        public IUICallBacks CallBacks => _uICallBacks;

        #endregion

        #region Properties
        private Animator _animator;
        #endregion

        #region MonoBehaviour Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var item in _uIElements)
            {
                if (item.element && string.IsNullOrEmpty(item.key)) item.key = item.element.name;
            }
        }
#endif

        // Start is called before the first frame update
        protected virtual void Start()
        {
            CallBackManager.RegisterCallback(this);
            _animator = GetComponent<Animator>();
            _uIElements.RegisterElements();

        }

        protected virtual void OnDestroy()
        {
            CallBackManager.UnregisterCallback(this);
            _uIElements.UnregisterElements();
        }

        // Update is called once per frame
        protected virtual void Update()
        {

        }

        #endregion

        #region System Callback Methods

        public virtual void OnExperienceFinished(bool isRunning) { }

        public virtual void OnExperienceReady() { }

        public virtual void OnExperienceStarted() { }

        public virtual void OnExperienceStarting() { }

        public virtual void OnHudStatusChanged(bool status) { }

        public virtual void OnLoadExtensionMenu(GameObject menu) { }

        public virtual void OnLoadScene()
        {
            if (_animator) _animator.SetBool("retracted", false);
            _loadingScreen.SetActive(false);
        }

        public virtual void OnLoadSceneFeature() { }

        public virtual void OnPlatformInitialize() { }

        public virtual void OnStartLoadScene()
        {
            if (_animator) _animator.SetBool("retracted", true);
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
            if (_animator) _animator.SetBool("retracted", status);
        }

        public virtual void OnOrientationChanged(ScreenOrientation orientation)
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
            GetElement(key)?.SetActive(true);
        }


    }

}
