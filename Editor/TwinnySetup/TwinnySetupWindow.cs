#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Concept.Helpers;
using Meta.XR.InputActions;
using Twinny.System;
using Twinny.XR;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;
using static System.Collections.Specialized.BitVector32;

namespace Twinny.Editor
{
    public class TwinnySetupWindow : EditorWindow
    {
        public readonly struct PackageInfo
        {
            public string name { get; }
            public string url { get; }

            public PackageInfo(string name, string url)
            {
                this.name = name;
                this.url = url;
            }
        }


        private const string PLUS_ICON_PATH = "project://database/Packages/com.twinny.twe25/Editor/TwinnySetup/src/Sprites/SetupIcons.png?fileID=-6071313599950475169&amp;guid=cc885f87ee614d74dbc45e6d1289404b&amp;type=3#ICO_Plus";

        private static readonly Dictionary<Platform, List<PackageInfo>> PLATFORM_DEPENDENCIES = new()
        {
            [Platform.MOBILE] = new List<PackageInfo>
    {
        new PackageInfo("com.conceptfactory.core", ""),
        new PackageInfo("com.unity.cinemachine", "com.unity.cinemachine")
    },

            [Platform.XR] = new List<PackageInfo>
    {
        new PackageInfo("com.unity.inputsystem", ""),
        new PackageInfo("com.unity.render-pipelines.universal", "")
    }
        };


        private static Vector2 _windowSize = new Vector2(800, 600);

        [SerializeField] private TwinnySetupConfig _config;
        private VisualElement m_root;
        private VisualElement m_SideBar;
        private VisualElement m_MainContent;

        private List<Section> m_sections = new List<Section>();

        private static string m_RootPath;


        private Sprite _plusIcon;

        [MenuItem("Twinny/Setup &T")]
        public static void Open()
        {
            var wnd = CreateInstance<TwinnySetupWindow>();
            wnd.titleContent = new GUIContent("Twinny Engine 2025");
            wnd.minSize = wnd.maxSize = _windowSize;
            wnd.ShowUtility();
            var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(wnd));
        }
        private void OnEnable()
        {
            var script = MonoScript.FromScriptableObject(this);
            var fullPath = AssetDatabase.GetAssetPath(script);
            m_RootPath = Path.GetDirectoryName(fullPath);
            _plusIcon = ImageUtils.LoadSpriteFromProjectURL(PLUS_ICON_PATH);
        }

        public void CreateGUI()
        {
            // Carrega o layout
            m_root = _config.visualTreeAsset.Instantiate();
            m_root.style.flexGrow = 1;
            rootVisualElement.Add(m_root);

            // Pega referências
            m_SideBar = m_root.Q<VisualElement>("Sidebar");
            m_MainContent = m_root.Q<VisualElement>("mainContent");
            var versionInfo = m_root.Q<Label>("VersionInfo");
            versionInfo.text = "v" + GetPackageVersion();

            InitSections();
        }

        private void InitSections()
        {
            foreach (var section in _config.sections)
            {
                m_sections.Add(CreateSection(section));
            }

            ShowSection("welcome");
        }

        private Section CreateSection(VisualTreeAsset layout)
        {
            VisualElement root = layout.CloneTree();
            Section section = root.Q<Section>();
            section.sectionLayout = layout;

            bool enabled = section.sectionPlatform == Platform.UNKNOW;

#if TWINNY_OPENXR
            if (section.sectionPlatform == Platform.XR) enabled = true;
#endif

#if TWINNY_MOBILE
            if (section.sectionPlatform == Platform.MOBILE) enabled = true;
#endif
#if TWINNY_WEBGL
            if (section.sectionPlatform == Platform.WEBGL) enabled = true;
#endif
#if TWINNY_WIN
            if (section.sectionPlatform == Platform.WINDOWS) enabled = true;
#endif


            section.sectionButton = AddSidebarButton(
                    section.sectionTitle,
                    section.sectionIcon,
                    _plusIcon,
                    () => ShowSection(section.sectionName),
                    enabled
                    );

            return section;
        }

        /*
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

        */
        private VisualElement AddSidebarButton(string title, Sprite iconTexture, Sprite plusTexture, Action onClick, bool enabled)
        {
            var button = _config.sideBarElement.Instantiate();
            button.RegisterCallback<ClickEvent>(evt =>
            {
                if (button.ClassListContains("disabled"))
                {
                    InstallPlatformRequest(title);
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


        void ShowSection(string name)
        {
            name = name.ToLowerInvariant();
            var section = m_sections.FirstOrDefault(s => s.sectionName.ToLowerInvariant() == name);
            ShowSection(section);

            switch (name)
            {
                case "welcome":
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

                case "openxr":
                    RuntimeXR runtimeXR = AssetDatabase.LoadAssetAtPath<RuntimeXR>("Assets/Resources/RuntimeXRPreset.asset");
                    DrawScriptable(runtimeXR, m_MainContent.Q<VisualElement>("runtimeConfig"));
                    break;

                case "mobile":
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
                var clone = section.sectionLayout.CloneTree();
                clone.style.flexGrow = 1f;
                m_MainContent.Add(clone);
            }
            SelectButton(section.sectionButton);
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

        void ShowInstallDialog(string caption, List<string> dependencies, Action onAgree)
        {
            var overlay = m_root.Q<VisualElement>("modalOverlay");
            overlay.style.display = DisplayStyle.Flex;
            var label = overlay.Q<Label>("caption");
            label.text = caption;

            var listView = overlay.Q<ListView>("packagesList");

            listView.itemsSource = dependencies;
            listView.makeItem = () => new Label();
            listView.bindItem = (element, i) => (element as Label).text = dependencies[i];

            listView.SetEnabled(false);  // desabilita o controle visualmente
            listView.selectionType = SelectionType.None;
            listView.selectionChanged += _ => listView.ClearSelection();


            m_root.Q<Button>("buttonInstall").clicked += onAgree;

            m_root.Q<Button>("buttonCancel").clicked += () =>
            {
                overlay.style.display = DisplayStyle.None;
            };

        }

        public void InstallPlatformRequest(string platformName)
        {
            var items = new List<string>(); 
            //{ "\u2714com.meta.all-in-one", "\u274Ccom.xvideos.api", "\u274Ccom.bolachinha.maria" };


            if (PLATFORM_DEPENDENCIES.TryGetValue(Platform.MOBILE, out var packages))
            {
                foreach (var pkg in packages)
                {
                    items.Add(pkg.name);
                }
            }

            ShowInstallDialog(
                $"<align=center><b>Deseja instalar a plataforma {platformName}?</b></align>\n\nOs seguintes pacotes precisam ser instalados:",
                items,
                () => InstallPlatform(platformName)
                );

            switch (platformName)
            {
                case "":
                    break;
            }
        }


        public void InstallPlatform(string platformName)
        {
            Debug.Log($"Install {platformName} platform");
        }
    }
}
#endif