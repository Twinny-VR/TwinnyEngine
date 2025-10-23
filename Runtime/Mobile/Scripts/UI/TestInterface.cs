using System;
using System.Collections.Generic;
using System.Linq;
using Concept.UI;
using Twinny.Addressables;
using Twinny.System;
using Twinny.System.Cameras;
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
            m_cutoffSlider.RegisterCallback<ChangeEvent<float>>(evt => MobileLevelManager.OnCutoffChanged(evt.newValue));


        }

        private void OnDisable()
        {

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
            
            await MobileLevelManager.Instance.ChangeScene(sceneName, 0);

            m_cutoffSlider.highValue = m_cutoffSlider.value = 1;
            m_imersiveButton.EnableInClassList("selected",false);
        }

        private async void SetFPS()
        {
            m_imersiveButton.EnableInClassList("selected",!FirstPersonAgent.isActive);
            m_cutoffSlider.style.display = FirstPersonAgent.isActive ? DisplayStyle.Flex : DisplayStyle.None;
            await MobileLevelManager.GetInstance()?.SetFPSAsync();
            m_cutoffSlider.highValue = m_cutoffSlider.value = 1;
        }





    }

}

