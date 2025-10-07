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

namespace Twinny.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainInterface : MonoBehaviour, IUICallBacks
    {
        private static MobileRuntime m_config => TwinnyRuntime.GetInstance<MobileRuntime>();


        public UIDocument document { get; private set; }
        private VisualElement m_root;
        private VisualElement m_mainContent;
        private VisualElement m_cardsContainer;

        private Foldout m_descriptionFoldout;

        #region MonoBehaviour Methods
        private void OnEnable()
        {
            document = GetComponent<UIDocument>();

            m_root = document?.rootVisualElement;
            m_mainContent = m_root?.Q<VisualElement>("MainContent");
            m_cardsContainer = m_root?.Q<VisualElement>("ProjectCardsContainer");

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

        private async void Start()
        {
            var init = await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;
            Debug.Log("[Addressables] Catálogos carregados:");

            ProjectList projectList = await AddressablesManager.LoadSeparateAssetAsync<ProjectList>("project_list");

            List<ProjectInfo> projectInfos = new List<ProjectInfo>(); 
           
            foreach (var id in projectList.projectGroups)
            {
                Debug.LogWarning($"Downloading Group '{id}'.");
                
                ProjectInfo projectInfo = await AddressablesManager.LoadSeparateAssetAsync<ProjectInfo>(id);

                Debug.LogWarning("INFO:" + projectInfo);
                if (projectInfo)
                {
                    Debug.LogWarning($"ProjectInfo '{id}' encontrado.");
                    if(projectInfo != null) projectInfos.Add(projectInfo);
                } else
                    Debug.LogError($"ProjectInfo '{id}' não encontrado.");
            }     
            
            Debug.LogWarning("PROJECTS TOTAL: "+projectInfos.Count);

            _=FillCardsContainer(projectInfos);
            

            _ = CanvasTransition.FadeScreen(false, m_config.fadeTime);
        }
        #endregion

        #region Private Methods

        private async Task FillCardsContainer(List<ProjectInfo> projectInfos)
        {
            m_cardsContainer.Clear();
            foreach (var info in projectInfos) {


                Texture2D thumb = await AddressablesManager.LoadSeparateAssetAsync<Texture2D>(info.thumbnailRef);
                Debug.Log($"thumb: {thumb.name}, width: {thumb.width}, height: {thumb.height}, isReadable: {thumb.isReadable}");
                /*

                Texture2D thumb = null;

                if(info.thumbnail != null && info.thumbnail.RuntimeKeyIsValid())
                {
                    var handle = info.thumbnail.LoadAssetAsync<Texture2D>();
                    await handle.Task;
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {

                        thumb = handle.Result;
                        Debug.LogWarning("Thumb: "+ thumb);
                    }
                    else
                        Debug.LogError($"Falha ao carregar thumbnail de {info.projectName}");
                }
                */
                CardElement card = new CardElement()
                {
                    title = info.projectName,
                    description = info.description,
                    thumbnail = thumb
                };

                card.OnClickEvent += () => selectProject(info.addressableKey);
                m_cardsContainer.Add(card);

            
            }
        }

        private void selectProject(string projectKey)
        {
            Debug.LogWarning($"'{projectKey}' Selected.");
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