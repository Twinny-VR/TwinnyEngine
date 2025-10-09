using Concept.Core;
using Concept.Addressables;
using Concept.UI;
using Twinny.Addressables;
using Twinny.System;
using UnityEngine;
using UnityEngine.UIElements;
using static Twinny.System.TwinnyManager;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.Rendering;
using System;

namespace Twinny.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainInterface : MonoBehaviour, IUICallBacks
    {
        private static MobileRuntime m_config => TwinnyRuntime.GetInstance<MobileRuntime>();


        public UIDocument document { get; private set; }
        private VisualElement m_root;
        private VisualElement m_logo;

        private bool m_logoExpanded;
        public bool logoExpanded
        {
            get => m_logoExpanded;
            set
            {
                m_logoExpanded = value;
                m_logo.EnableInClassList("expanded", value);
            }
        }

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
        private Button m_homeButton;

        private VisualElement m_mainContent;
        private VisualElement m_mainCardsContainer;


        private VisualElement m_projContent;
        private AspectElement m_projBanner;
        private Label m_titleLabel;
        private Label m_authorLabel;
        private Foldout m_descriptionFoldout;
        private Label m_descriptionLabel;
        private VisualElement m_projCardsContainer;


        #region MonoBehaviour Methods
        private void OnEnable()
        {
            document = GetComponent<UIDocument>();

            m_root = document?.rootVisualElement;
            m_logo = m_root.Q<VisualElement>("Logo");
            m_footer = m_root.Q<VisualElement>("Footer");
            m_homeButton = m_footer.Q<Button>("ButtonHome");
            m_homeButton.clicked += BackToHome;

            m_mainContent = m_root?.Q<VisualElement>("MainContent");
            m_mainCardsContainer = m_mainContent?.Q<VisualElement>("ProjectCardsContainer");
            
            //Projects Guide
            m_projContent = m_root?.Q<VisualElement>("ProjectContent");
            m_projBanner = m_projContent.Q<AspectElement>("Banner");

            m_titleLabel = m_projContent.Q<Label>("TitleLabel");
            m_authorLabel = m_projContent.Q<Label>("AuthorLabel");

            m_projCardsContainer = m_projContent?.Q<VisualElement>("ProjectCardsContainer");
            m_descriptionFoldout = m_projContent.Q<Foldout>("DescriptionFoldout");
            m_descriptionLabel = m_projContent.Q<Label>("DescriptionLabel");

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

        private ProjectList m_projectList;
           private List<ProjectInfo> m_projectInfos = new List<ProjectInfo>(); 

        private async void Start()
        {
            var init = await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;
            Debug.Log("[Addressables] Catálogos carregados:");

            ProjectList originalList = await AddressablesManager.LoadSeparateAssetAsync<ProjectList>("project_list");
            if(originalList!=null) m_projectList = ScriptableObject.Instantiate(originalList);

           
            foreach (var id in m_projectList.projectGroups)
            {
                Debug.LogWarning($"Downloading Group '{id}'.");
                
                ProjectInfo originalInfo = await AddressablesManager.LoadSeparateAssetAsync<ProjectInfo>(id);

                if (originalInfo == null) continue;
                ProjectInfo projectInfo = ScriptableObject.Instantiate(originalInfo);

                Debug.LogWarning("INFO:" + projectInfo);
                if (projectInfo)
                {
                    Debug.LogWarning($"ProjectInfo '{id}' encontrado.");
                    m_projectInfos.Add(projectInfo);
                } else
                    Debug.LogError($"ProjectInfo '{id}' não encontrado.");
            }     
            
            Debug.LogWarning("PROJECTS TOTAL: "+m_projectInfos.Count);

            _=FillCardsContainer();
            

            _ = CanvasTransition.FadeScreen(false, m_config.fadeTime);
        }
        #endregion

        #region Private Methods

        private async Task FillCardsContainer()
        {
            m_mainCardsContainer.Clear();
            foreach (var info in m_projectInfos) {


                Texture2D thumb = await AddressablesManager.LoadSeparateAssetAsync<Texture2D>(info.thumbnailRef);
                
                CardElement card = new CardElement()
                {
                    title = info.projectName,
                    description = info.description,
                    thumbnail = thumb
                };

                card.OnClickEvent += () => _=selectProject(info);
                m_mainCardsContainer.Add(card);

            
            }
        }

        Texture2D CreateRuntimeTexture(Texture2D src)
        {
            Texture2D tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            tex.SetPixels(src.GetPixels());
            tex.Apply();
            return tex;
        }

        private async Task selectProject(ProjectInfo info)
        {
            m_projContent.style.display = DisplayStyle.Flex;
            m_mainContent.style.display = DisplayStyle.None;
            logoExpanded = footerExpanded = true;

            m_titleLabel.text = info.projectName;
            m_authorLabel.text = info.authorName;

            m_descriptionFoldout.text = $"<b>{info.projectName}</b><br>{info.authorName}";
            m_descriptionLabel.text = info.description;


            m_projCardsContainer.Clear();
            Texture2D thumb = await AddressablesManager.LoadSeparateAssetAsync<Texture2D>(info.thumbnailRef);


            m_projBanner.SetBackgroundTexture(CreateRuntimeTexture(thumb));
            
            //TODO Aprimorar
            /*
            VideoClip clip = await AddressablesManager.LoadSeparateAssetAsync<VideoClip>(info.videoRef);

            if (clip != null) {
                VideoElement video = new VideoElement()
                {
                    expanded = false,
                    videoClip = clip,
                    autoPlay = true,
                    looping = true 
                };
                m_projCardsContainer.Add(video);
            }
            */

            foreach (var key in info.galeryRef) { 
                Texture2D pic = await AddressablesManager.LoadSeparateAssetAsync<Texture2D>(key);
                
                if(pic != null)
                {
                    CardElement card = new CardElement()
                    {
                        expanded = false,
                        thumbnail = pic
                    };
                    card.OnClickEvent += () => MaximizeCard(card);
                    m_projCardsContainer.Add(card);
                }
                else
                    Debug.LogError($"[MainInterface] Impossible to load '{key} texture resource.");

            }



        }

        private void MaximizeCard(CardElement card)
        {
            Vector2 worldPos = card.worldBound.position;
            Vector2 size = card.worldBound.size;
            float rootWidth = m_root.resolvedStyle.width;
            float rootHeight = m_root.resolvedStyle.height;


            CardElement clone = new CardElement()
            {
                expanded = false,
                thumbnail = card.thumbnail
            };

            clone.AddToClassList("clone");
            m_root.Add(clone);


            clone.style.position = Position.Absolute;
            clone.style.left = worldPos.x;
            clone.style.top = worldPos.y;
            clone.style.right = rootWidth - (worldPos.x + size.x);
            clone.style.bottom = rootHeight - (worldPos.y + size.y);

            Debug.LogWarning(card+" clicked.");
            clone.schedule.Execute(_ =>
            {
                StartExpandAnimation(clone, worldPos, size, rootWidth, rootHeight);
                // clone.AddToClassList("expanded"); // anima para ocupar 32px de borda
            }).ExecuteLater(1);
        }

        private void StartExpandAnimation(VisualElement element, Vector2 startPos, Vector2 startSize, float rootWidth, float rootHeight)
        {
            // Estado final
            float targetLeft = 32f;
            float targetTop = 32f;
            float targetWidth = rootWidth - 64f;
            float targetHeight = rootHeight - 64f;

            // Duração em milissegundos
            float duration = 500f;
            float startTime = Time.time;

            // Função de atualização da animação
            Action<TimerState> updateAnimation = null;
            updateAnimation = (state) =>
            {
                float elapsed = (Time.time - startTime) * 1000f; // Convert to milliseconds
                float progress = Mathf.Clamp01(elapsed / duration);

                // Easing function
                float easedProgress = EaseOutCubic(progress);

                // Interpola os valores
                element.style.left = Mathf.Lerp(startPos.x, targetLeft, easedProgress);
                element.style.top = Mathf.Lerp(startPos.y, targetTop, easedProgress);
                element.style.width = Mathf.Lerp(startSize.x, targetWidth, easedProgress);
                element.style.height = Mathf.Lerp(startSize.y, targetHeight, easedProgress);

                // Continua animando se não terminou
                if (progress < 1f)
                {
                    element.schedule.Execute(updateAnimation).ExecuteLater(16); // ~60fps
                }
            };

            // Inicia a animação
            element.schedule.Execute(updateAnimation).ExecuteLater(16);
        }

        private float EaseOutCubic(float x)
        {
            return 1f - Mathf.Pow(1f - x, 3f);
        }


        private void BackToHome()
        {
            m_mainContent.style.display = DisplayStyle.Flex;
            m_projContent.style.display = DisplayStyle.None;
            logoExpanded = footerExpanded = false;


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