#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Meta.XR.InputActions;
using Twinny.System;
using Twinny.XR;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

namespace Twinny.Editor
{


    public class TwinnySetupWindow : EditorWindow
    {
        private static Vector2 _windowSize = new Vector2(800, 600);

        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField] private VisualTreeAsset m_SideBarElement = default;
        [SerializeField] private VisualTreeAsset m_WelcomeTreeAsset = default;
        [SerializeField] private VisualTreeAsset m_OpenXRTreeAsset = default;
        [SerializeField] private VisualTreeAsset m_MobileTreeAsset = default;
        private VisualElement m_SideBar;
        private VisualElement m_MainContent;

        private List<Section> m_sections = new List<Section>();

        private static string m_RootPath;

        [MenuItem("Twinny/Setup &T")]
        public static void Open()
        {
            TwinnySetupWindow wnd = GetWindow<TwinnySetupWindow>();
            wnd.titleContent = new GUIContent("Twinny Engine 2025");
            wnd.minSize = _windowSize;
            wnd.maxSize = _windowSize;
            var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(wnd));
        }
        private void OnEnable()
        {
            var script = MonoScript.FromScriptableObject(this);
            var fullPath = AssetDatabase.GetAssetPath(script);
            m_RootPath = Path.GetDirectoryName(fullPath);
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
            var versionInfo = root.Q<Label>("VersionInfo");
            versionInfo.text = "v" + GetPackageVersion();

            InitSections();
        }

        private void InitSections()
        {
            m_sections.Add(new Section()
            {
                name = "Welcome",
                title = "TWINNY ENGINE",
                layout = m_WelcomeTreeAsset,
                button = AddSidebarButton("", "ICO_Twinny", () => ShowSection("Welcome"))
            });


            m_sections.Add(new Section()
            {
                name = "OpenXr",
                title = "Open XR",
                layout = m_OpenXRTreeAsset,
                button = AddSidebarButton(
                    "Open XR",
                    "ICO_XRPlatform",
                    () => ShowSection("OpenXr")
#if !TWINNY_OPENXR
                    , false
#endif
                    )
            });

            m_sections.Add(new Section()
            {
                name = "Mobile",
                title = "Mobile",
                layout = m_MobileTreeAsset,
                button = AddSidebarButton(
                    "Mobile",
                    "ICO_MobilePlatform",
                    () => ShowSection("Mobile")
#if !TWINNY_MOBILE
                    , false
#endif
                    )
            });


            m_sections.Add(new Section()
            {
                name = "WebGL",
                title = "WebGL",
                layout = m_WelcomeTreeAsset,
                button = AddSidebarButton(
                    "WebGL",
                    "ICO_WebGLPlatform",
                    () => ShowSection("WebGL")
#if !TWINNY_WEBGL
                    , false
#endif
                    )
            });

            m_sections.Add(new Section()
            {
                name = "Win",
                title = "Windows",
                layout = m_WelcomeTreeAsset,
                button = AddSidebarButton(
                    "Windows",
                    "ICO_WinPlatform",
                    () => ShowSection("Win")
#if !TWINNY_WIN
                    , false
#endif
                    )
            });

            ShowSection("Welcome");

        }

        private VisualElement AddSidebarButton(string title, string spriteName, Action onClick, bool enabled = true)
        {
            string atlasPath = "SetupIcons.png";
            var path = Path.Combine(m_RootPath, "src", "Sprites", atlasPath).Replace("\\", "/");

            var sprites = LoadSpritesFromAtlas(path);
            if (sprites == null)
            {
                throw new Exception($"[TwinnySetupWindow] Atlas not found in '{path}'");
            }
            Sprite sprite = sprites.FirstOrDefault(s => s.name == spriteName);
            if (sprite == null)
            {
                throw new Exception($"[TwinnySetupWindow] No sprite '{spriteName}' found on atlas '{path}'");
            }

            Sprite plus = sprites.FirstOrDefault(s => s.name == "ICO_Plus");
            if (plus == null)
            {
                throw new Exception($"[TwinnySetupWindow] No 'ICO_Plus' texture found on atlas '{path}'");
            }


            return AddSidebarButton(title, sprite, plus, onClick, enabled);
        }


        private VisualElement AddSidebarButton(string title, Sprite iconTexture, Sprite plusTexture, Action onClick, bool enabled)
        {
            var button = m_SideBarElement.Instantiate();
            button.RegisterCallback<ClickEvent>(evt =>
            {
                if (button.ClassListContains("disabled"))
                {
                    InstallPlatform(title);
                    return;//TODO Install Platform
                }
                SelectButton(button);
                onClick?.Invoke();
            });
            if (!enabled)
                button.tooltip = $"Install {title} platform";

            button.AddToClassList("sidebar-button");
            var label = button.Q<Label>("label");
            var icon = button.Q<Image>("icon");
            var plus = button.Q<Image>("plus");
            label.text = title;

            if (iconTexture != null)
            {
                icon.sprite = iconTexture;
            }
            plus.sprite = plusTexture;

            button.AddToClassList(enabled ? "enabled" : "disabled");

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
            public string name;
            public string title;
            public VisualElement button;
            public VisualTreeAsset layout;
        }

        void ShowSection(string name)
        {
            var section = m_sections.FirstOrDefault(s => s.name == name);
            ShowSection(section);

            switch (name)
            {
                case "Welcome":
                    var productField = m_MainContent.Q<TextField>("ProductNameField");
                    productField.value = PlayerSettings.productName;
                    var companyField = m_MainContent.Q<TextField>("CompanyNameField");
                    var versionField = m_MainContent.Q<TextField>("VersionField");
                    versionField.value = PlayerSettings.bundleVersion;
                    companyField.RegisterValueChangedCallback(evt => { PlayerSettings.companyName = evt.newValue; });
                    productField.RegisterValueChangedCallback(evt => { PlayerSettings.productName = evt.newValue; });
                    versionField.RegisterValueChangedCallback(evt => { PlayerSettings.bundleVersion = evt.newValue; });
                    var company = PlayerSettings.companyName == "DefaultCompany" ? "Twinny VR" : Application.companyName;
                    companyField.value = company;
                    break;

                    case "OpenXr":
                    RuntimeXR runtimeXR = AssetDatabase.LoadAssetAtPath<RuntimeXR>("Assets/Resources/RuntimeXRPreset.asset");
                    DrawScriptable(runtimeXR, m_MainContent.Q<VisualElement>("runtimeConfig"));
                    break;

                    case "Mobile":
                    MobileRuntime runtimeMobile = AssetDatabase.LoadAssetAtPath<MobileRuntime>("Assets/Resources/MobileRuntimePreset.asset");
                    DrawScriptable(runtimeMobile, m_MainContent.Q<VisualElement>("runtimeConfig"));
                    break;
            }



        }

        void ShowSection(Section section)
        {
            if (section == null) return;

            m_MainContent.Clear(); // limpa conteúdo antigo
            if (section.layout != null)
            {
                var clone = section.layout.CloneTree();
                clone.style.flexGrow = 1f;
                m_MainContent.Add(clone);
            }
            SelectButton(section.button);
        }

        void DrawScriptable(UnityEngine.Object scriptable, VisualElement element)
        {

            InspectorElement inspector = new InspectorElement(scriptable);

            element.Add(inspector);
        }
        public static Sprite[] LoadSpritesFromAtlas(string path)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            var sprites = assets.OfType<Sprite>().ToArray();

            return sprites;
        }


        public static string GetPackageVersion()
        {
            var pkgInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TwinnyManager).Assembly);
            return pkgInfo != null ? pkgInfo.version : "?.?.?";
        }


        public static void InstallPlatform(string platformName)
        {
            Debug.Log($"Install {platformName} platform");

            switch (platformName)
            {
                case "":
                    break;
            }
        }

        public static void InstallOpenXR() { 
        }
    }
}
#endif