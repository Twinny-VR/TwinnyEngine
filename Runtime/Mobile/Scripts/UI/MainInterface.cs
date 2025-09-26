using Concept.Core;
using Concept.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Twinny.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainInterface : MonoBehaviour, IUICallBacks
    {
        public UIDocument document { get; private set; }
        private VisualElement m_root;
        private VisualElement m_mainContent;

        private Foldout m_descriptionFoldout;

        #region MonoBehaviour Methods
        private void OnEnable()
        {
            document = GetComponent<UIDocument>();

            m_root = document?.rootVisualElement;
            m_mainContent = m_root?.Q<VisualElement>("MainContent");

            m_descriptionFoldout = m_root.Q<Foldout>("DescriptionFoldout");
            ResponsiveElement nearestDeepResponsive = m_descriptionFoldout.GetFirstAncestorOfType<ResponsiveElement>();
            if (nearestDeepResponsive != null) nearestDeepResponsive.OnResize += (isLandscape) => {

                m_descriptionFoldout.value = isLandscape;
                m_descriptionFoldout.SetEnabled(!isLandscape);

            };
            CallbackHub.RegisterCallback(this);
        }

        private void OnDisable()
        {
            CallbackHub.UnregisterCallback(this);
        }
        #endregion

        #region UI Callback Methods

        public void OnHudStatusChanged(bool status) {}

        public void OnPlatformInitialize() {}

        public void OnExperienceReady() {}

        public void OnExperienceFinished(bool isRunning) {}

        public void OnLoadExtensionMenu(GameObject menu, bool isStatic = false)
        {
        }

        public void OnStartLoadScene()
        {
            m_mainContent.style.display = DisplayStyle.None;
        }

        public void OnLoadScene()
        {
        }

        public void OnLoadSceneFeature()
        {
        }

        public void OnUnloadSceneFeature()
        {
        }

        public void OnExperienceStarting()
        {
        }

        public void OnExperienceStarted()
        {
        }

        public void OnSwitchManager(int source)
        {
        }

        public void OnStandby(bool status)
        {
        }

        public void OnCameraChanged(Transform camera, string type)
        {
        }

        public void OnCameraLocked(Transform target)
        {
        }

        #endregion
    }

}