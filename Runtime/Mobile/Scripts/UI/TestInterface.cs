using System;
using System.Collections.Generic;
using System.Linq;
using Concept.UI;
using Twinny.Addressables;
using Twinny.System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Twinny.UI
{


    [RequireComponent(typeof(UIDocument))]

    public class TestInterface : MonoBehaviour
    {
        private static MobileRuntime m_config => TwinnyRuntime.GetInstance<MobileRuntime>();


        public UIDocument document { get; private set; }
        private VisualElement m_root;


        private DropdownField m_scenesDropdown;


        //Right Menu
        private VisualElement m_rightMenu;
        private Slider m_cutoffSlider;


        //Footer
        private VisualElement m_footer;
        private bool m_footerExpanded;
        public bool footerExpanded
        {
            get => m_footerExpanded;
            set
            {
                m_footerExpanded = value;
                m_footer.EnableInClassList("expanded", value);
            }
        }


        private Button m_enterButton;
        private Button m_imersiveButton;


        [DrawScriptable]
        public ProjectScenes projectScenes;

        private void OnEnable()
        {
            document = GetComponent<UIDocument>();

            m_root = document?.rootVisualElement;

            m_scenesDropdown = m_root.Q<DropdownField>("ScenesDropdown");
            m_scenesDropdown.RegisterValueChangedCallback(evt =>
            {
                OnSceneSelected(projectScenes.sceneInfos.FirstOrDefault(c => c.sceneDisplayName == evt.newValue).sceneName);
            });



            m_footer = m_root.Q<VisualElement>("Footer");
            m_enterButton = m_footer.Q<Button>("ButtonEnter");
           // m_enterButton.clicked += StartExperience;
            m_imersiveButton = m_footer.Q<Button>("ButtonImersive");
            m_imersiveButton.clicked += SetFPS;



            m_root.schedule.Execute(() =>
            {
                FillSceneList();
                m_scenesDropdown.index = 0;
            }).StartingIn(1);


            //Right Menu
            m_rightMenu = m_root.Q<VisualElement>("RightMenu");
            m_cutoffSlider = m_root.Q<Slider>("CutoffSlider");
            m_cutoffSlider.RegisterCallback<ChangeEvent<float>>(evt => MainInterface.OnCutoffChanged?.Invoke(evt.newValue));

            MainInterface.OnCutoffChanged += OnCutoffChanged;

        }

        private void OnDisable()
        {
            MainInterface.OnCutoffChanged -= OnCutoffChanged;

        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _ = CanvasTransition.FadeScreen(false, m_config.fadeTime);

        }

        private void FillSceneList()
        {
            m_scenesDropdown.choices = new List<string>();
            foreach (var scene in projectScenes.sceneInfos)
            {
                m_scenesDropdown.choices.Add(scene.sceneDisplayName);
            }
        }

        private async void OnSceneSelected(string sceneName)
        {
            Shader.SetGlobalFloat("_CutoffHeight", 4.5f);
            m_enterButton.style.display = DisplayStyle.None;

            await MobileLevelManager.Instance.ChangeScene(sceneName, 0);
            m_imersiveButton.style.display = DisplayStyle.Flex;
        }

        private async void SetFPS()
        {
            m_imersiveButton.style.display = DisplayStyle.None;
            await MobileLevelManager.GetInstance()?.SetFPSAsync();
            m_enterButton.style.display = DisplayStyle.Flex;

        }


        private void OnCutoffChanged(float value)
        {
            Shader.SetGlobalFloat("_CutoffHeight", value);

        }


    }

}

