#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Twinny.Editor
{


    public class TwinnySetupWindow : EditorWindow
    {
        private static Vector2 _windowSize = new Vector2(800, 600);

        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField] private Texture2D m_WelcomeIcon;
        [SerializeField] private VisualTreeAsset m_WelcomeTreeAsset = default;
        [SerializeField] private VisualTreeAsset m_OpenXRTreeAsset = default;
        private VisualElement m_SideBar;
        private VisualElement m_MainContent;

        private List<Section> m_sections = new List<Section>();

        [MenuItem("Window/UI Toolkit/TwinnySetup")]
        [MenuItem("Twinny/Setup")]
        public static void Open()
        {
            TwinnySetupWindow wnd = GetWindow<TwinnySetupWindow>();
            wnd.titleContent = new GUIContent("Twinny Engine 2025");
            wnd.minSize = _windowSize;
            wnd.maxSize = _windowSize;
        }
        private void OnEnable()
        {
        }

        public void CreateGUI()
        {
            // Carrega o layout
            VisualElement root = m_VisualTreeAsset.Instantiate();
            root.style.flexGrow = 1;
            rootVisualElement.Add(root);

            // Pega referências
            m_SideBar = root.Q<VisualElement>("Sidebar");
            m_MainContent = root.Q<VisualElement>("mainContent");

            InitSections();
        }

        private void InitSections()
        {
            m_sections.Add(new Section()
            {
                title = "Welcome",
                layout = m_WelcomeTreeAsset,
                button = AddSidebarButton("Bem-vindo", () => ShowSection("Welcome"))
            });


#if TWINNY_OPENXR
            m_sections.Add(new Section()
            {
                title = "OpenXr",
                layout = m_OpenXRTreeAsset,
                button = AddSidebarButton("Configurar OpenXR", () => ShowSection("OpenXr"))
            });
#endif

            ShowSection("Welcome");

        }

        private VisualElement AddSidebarButton(string title, Action onClick)
        {
            var button = new VisualElement();
            button.RegisterCallback<ClickEvent>(evt =>
            {
                SelectButton(button);
                onClick?.Invoke();
            });
            button.tooltip = title;
            button.AddToClassList("sidebar-button");

            var label = new Label(title);
            button.Add(label);

            m_SideBar.Add(button);
            return button;
        }

        private void SelectButton(VisualElement selectedButton)
        {
            foreach (var child in m_SideBar.Children())
            {
                child.RemoveFromClassList("selected");
            }
            selectedButton.AddToClassList("selected");
        }

        [Serializable]
        class Section
        {
            public string title;
            public VisualElement button;
            public VisualTreeAsset layout;
        }

        void ShowSection(string title)
        {
            var section = m_sections.FirstOrDefault(s => s.title == title);
            ShowSection(section);
        }

        void ShowSection(Section section)
        {
            if (section == null) return;

            m_MainContent.Clear(); // limpa conteúdo antigo
            if (section.layout != null)
            {
                var clone = section.layout.CloneTree();
                m_MainContent.Add(clone);
            }
            SelectButton(section.button);
        }


    }
}
#endif