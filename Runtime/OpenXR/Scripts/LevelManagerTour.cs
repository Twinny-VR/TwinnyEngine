using System;
using System.Threading;
using System.Threading.Tasks;
using Twinny.System;
using Twinny.UI;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using static Twinny.System.TwinnyManager;

namespace Twinny.GamePlay
{


    public class LevelManagerTour : MonoBehaviour
    {
        [SerializeField] private Transform m_cameraRigTransform;

        [SerializeField] private PlayableDirector m_timeline;

        [SerializeField] private Material[] m_skyboxMaterials;

        [SerializeField] private DynamicMeshSpawner m_spawner;
        private CancellationTokenSource m_spawnCancellation;

        [SerializeField] private LandMark[] m_landMarks;
        private LandMark m_currentLandMark;


        void Start()
        {
            InitializeScene();
        }



        private void OnDestroy()
        {
            m_spawnCancellation?.Cancel();
        }

        private async void InitializeScene()
        {

            TeleportToLandMark(0);

            try
            {
                m_spawnCancellation = new CancellationTokenSource();
                bool sceneLoad = await m_spawner.SpawnAsync(active: true, cancellationToken: m_spawnCancellation.Token).OnProgress((progress) =>
                {
                    Twinny.UI.AlertViewHUD.PostMessage($"CARREGANDO\n{(progress * 100).ToString("F0")}%");
                });
                Twinny.UI.AlertViewHUD.CancelMessage();

                if (!sceneLoad) Debug.LogError("UNKNOW ERROR SPAWNER SCENE OBJECTS!");
                else
                    await CanvasTransition.FadeScreen(false, 1f);

                m_timeline?.Play();

            }
            catch (TaskCanceledException)
            {
                Debug.LogWarning("Carregamento cancelado pelo usuário ou pelo sistema.");
            }
            catch (Exception ex)
            {

                Debug.LogError($"Erro ao spawnar os objetos: {ex}");
            }
        }


        public async void NavigateTo(int landMarkIndex)
        {
            m_timeline.Pause();
            await CanvasTransition.FadeScreen(true, 1f);
            TeleportToLandMark(landMarkIndex);
            await CanvasTransition.FadeScreen(false, 1f);
            m_timeline.Resume();
        }


        private void TeleportToLandMark(int landMarkIndex)
        {
            if (m_landMarks.Length < landMarkIndex) return;
            m_currentLandMark?.node?.OnLandMarkUnselected?.Invoke();
            m_currentLandMark = m_landMarks[landMarkIndex];

            if (m_currentLandMark == null) return;


            m_cameraRigTransform.position = m_currentLandMark.position;
            m_cameraRigTransform.rotation = m_currentLandMark.rotation;

            m_cameraRigTransform.SetParent(m_currentLandMark.node.changeParent ? m_currentLandMark.node.newParent : null);
            m_currentLandMark.node.OnLandMarkSelected?.Invoke();

            if(m_currentLandMark?.skyBoxMaterial != null) SetHDRI(m_currentLandMark.skyBoxMaterial);
            
        }

        private void SetHDRI(Material skyBoxMaterial)
        {
            if (RenderSettings.skybox != skyBoxMaterial)
            {
                RenderSettings.skybox = skyBoxMaterial;
                DynamicGI.UpdateEnvironment();
            }
        }

        public async void ChangeScene(string sceneName)
        {
            await CanvasTransition.FadeScreen(true, 1f);
            SceneManager.LoadScene(sceneName);
        }
        public async void ChangeScene(int scene)
        {
            await CanvasTransition.FadeScreen(true, 1f);
            SceneManager.LoadScene(scene);
        }

        public async void CarTeleport(int skyboxID)
        {
            await CanvasTransition.FadeScreen(true, 1f);
            
            if (m_skyboxMaterials.Length >= skyboxID)
                SetHDRI(m_skyboxMaterials[skyboxID]);

            await CanvasTransition.FadeScreen(false, 1f,.5f);
        }

    }

}