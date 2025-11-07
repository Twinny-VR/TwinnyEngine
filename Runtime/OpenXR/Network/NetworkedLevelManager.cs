using Concept.Core;
using Concept.Helpers;
using Fusion;
using System.Threading.Tasks;
using Twinny.System.Network;
using Twinny.UI;
using Twinny.XR;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Twinny.System
{
    using static TwinnyManager;


    [RequireComponent(typeof(NetworkObject))]
    public abstract class NetworkedLevelManager : NetworkBehaviour
    {

        #region Singleton Instance
        private static NetworkedLevelManager _instance;
        public static NetworkedLevelManager Instance { get { return _instance; } }
        #endregion
        private bool _isSpawned;

        #region Fields

        protected NetworkObject _networkObject;
        [Networked] public PlayerRef master { get; set; } = PlayerRef.None;

        [Space]
        [Header("Linked Components")]


        [SerializeField] protected bool _isManager = false;
        public static bool IsManager { get => Instance._isManager; }
        [Networked] public PlayerRef manager { get; set; } = PlayerRef.None;

        [Networked] public int currentLandMark { get; set; } = -1;



        #endregion

        #region Delegates
        #endregion

        public static bool isRunning = false;
        RuntimeXR config => TwinnyRuntime.GetInstance<RuntimeXR>();

        #region MonoBehaviour Methods


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (AssetDatabase.IsValidFolder("Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string fileName = "RuntimeXRPreset.asset";
            string assetPath = "Assets/Resources/" + fileName;
            NetworkRuntime preset = AssetDatabase.LoadAssetAtPath<NetworkRuntime>(assetPath);

            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<NetworkRuntime>();
                AssetDatabase.CreateAsset(preset, assetPath);
                AssetDatabase.SaveAssets();
                Debug.Log("Novo preset 'RuntimeXRPreset' criado e salvo em: " + assetPath);
            }




        }
#endif

        //Awake is called before the script is started
        protected virtual void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
            {
                Destroy(_instance);
                Debug.LogWarning("[LevelManager] Multiple instance removed.");
            }

            _networkObject = GetComponent<NetworkObject>();
            //   TwinnyManager.LoadRuntimeProfile<TwinnyRuntime>("RuntimePreset");

        }
        // Start is called before the first frame update
        protected virtual void Start()
        {
            _ = InitializePlatform();
        }

        // Update is called once per frame
        void Update()
        {
            if (_isSpawned && HasStateAuthority && master != NetworkRunnerHandler.runner.LocalPlayer)
            {
                SetMaster();
            }
        }
        // Destroy is called when component/object was removed
        private void OnDestroy()
        {
        }




        #endregion




        #region Public Methods

        protected virtual async Task InitializePlatform()
        {
            AsyncOperation loadScene = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            await AsyncOperationExtensions.WaitForSceneLoadAsync(loadScene);
            Initialize();

        }

        public virtual async Task ConnectToServer() { await Task.Yield();  }

        protected virtual void SetMaster()
        {
            master = NetworkRunnerHandler.runner.LocalPlayer;
            Debug.Log("[NetworkedLevelManager] You are the MASTER!");

        }

        public static void SetOwner(NetworkRunner runner)
        {
            if (Instance._networkObject != null)
                Instance._networkObject.SetPlayerAlwaysInterested(runner.LocalPlayer, true);

        }


        public virtual void GetReady()
        {
            isRunning = true;

            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnExperienceReady());
        }

        public static void StartExperience(string scene, int landMarkIndex)
        {
            Instance._isManager = true;

            Instance.RPC_StartForAll(NetworkRunnerHandler.runner.LocalPlayer, scene, landMarkIndex);
        }

        [ContextMenu("Quit Experience")]
        public static void Quit()
        {
            Instance.RPC_QuitForAll();

        }
        [ContextMenu("Quit Experience")]
        public static void Reset()
        {
            Instance._isManager = false;


            Instance.RPC_ResetForAll();
        }

        public virtual async Task ResetExperience()
        {
            if (UnityEngine.Application.isEditor) return;

            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnExperienceFinished(false));
            await CanvasTransition.FadeScreen(true, config.fadeTime);
        }

        public virtual async Task QuitExperience()
        {
            if (UnityEngine.Application.isEditor) return;

            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnExperienceFinished(false));
            await CanvasTransition.FadeScreen(true, config.fadeTime);

        }

        public virtual void Shutdown() { }

        public virtual void ResetApplication() { }

        /// <summary>
        /// This Async Method changes the actual scene.
        /// </summary>
        /// <param name="scene">Scene Name</param>
        /// <param name="landMarkIndex">First LandMark to teleport.</param>
        public async Task ChangeScene(string scene, int landMarkIndex)
        {
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnStartLoadScene());

            RPC_FadingStatus(1);
            RPC_Message(Runner.LocalPlayer, PlayerRef.None, "CARREGANDO CENA...", time: 90f);

            await CanvasTransition.FadeScreen(true, config.fadeTime);

            //TODO Mudar o sistema de carregamento de cenas
            if (scene == "PlatformScene" || scene == "OpenXR_PlatformScene")
            {
                await NetworkSceneManager.UnloadAdditivesScenes();
                RPC_RestartForAll();
            }
            else
            {
                await NetworkSceneManager.LoadAdditiveSceneAsync(scene, landMarkIndex);

            }

            RPC_Message(Runner.LocalPlayer, PlayerRef.None, "");

            SceneFeature sceneFeature = SceneFeature.Instance;
            if (sceneFeature == null )
                {
                RPC_FadingStatus(0);
                await CanvasTransition.FadeScreen(false, config.fadeTime);
                }
            }





        /// <summary>
        /// This method is only for handle the Fade Screen during the teleporting
        /// </summary>
        /// <param name="landMarkIndex">LandMark to Teleport</param>
        public async void NavigateTo(int landMarkIndex)
        {
            await CanvasTransition.FadeScreen(true, config.fadeTime);
            SceneFeature.Instance.TeleportToLandMark(landMarkIndex);
            await CanvasTransition.FadeScreen(false, config.fadeTime);
        }


        #endregion

        #region Private Methods
        #endregion

        #region Coroutines






        #endregion


        #region RPCs
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_FadingStatus(int status)
        {
            if (!NetworkRunnerHandler.runner.IsSceneAuthority) _ = CanvasTransition.FadeScreen(status == 1, config.fadeTime);
        }


        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_Message(PlayerRef source, PlayerRef target, string message, int messageType = 0, float time = 10f)
        {
            Twinny.UI.AlertViewHUD.MessageType type = (Twinny.UI.AlertViewHUD.MessageType)messageType;


            if (target == PlayerRef.None || target == Runner.LocalPlayer)
            {
                if (message != "")
                    Twinny.UI.AlertViewHUD.PostMessage(message, type, time);
                else
                    Twinny.UI.AlertViewHUD.CancelMessage();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_StartForAll(PlayerRef source, string scene, int landMark = -1)
        {
            currentLandMark = landMark;
            manager = source;

            _isManager = (source == NetworkRunnerHandler.runner.LocalPlayer);

            Debug.LogWarning(IsManager ? "YOU ARE THE MASTER!" : (source != PlayerRef.None ? $"{source} IS THE MASTER!" : "NO MASTERS"));

            if (source != PlayerRef.None)
                CallbackHub.CallAction<IUICallBacks>(callback => callback.OnExperienceStarting());


            if (NetworkRunnerHandler.runner.IsSceneAuthority)
                _ = ChangeScene(scene, landMark);

        }
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_QuitForAll()
        {
            _ = QuitExperience();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_ResetForAll()
        {
            _ = ResetExperience();
        }
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_RestartForAll()
        {
            CallbackHub.CallAction<IUIXRCallbacks>(callback => callback.OnExperienceFinished(isRunning));
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_ChangeScene(string scene, int landMark)
        {
            if (SceneManager.sceneCount > 1)//It means the Simulation is running
            {
                if (NetworkRunnerHandler.runner.IsSceneAuthority)
                    _ = ChangeScene(scene, landMark);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_MenuReset(bool reset)//TODO Substituir
        {
            //HUDManager.Instance.SetElementActive(reset || IsManager);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_SpawnPlayer(PlayerRef source)//TODO Ver se precisa
        {
            Debug.LogWarning("RPC_SpawnPlayer:" + source);

            if (NetworkRunnerHandler.runner.IsSceneAuthority)
            {
                //SpawnPlayer(source);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_NavigateTo(int landMarkIndex, bool fade = true)
        {
            currentLandMark = landMarkIndex;


            if (fade)
                NavigateTo(landMarkIndex);
            else
                SceneFeature.Instance?.TeleportToLandMark(landMarkIndex);

        }


        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_SwitchManager(PlayerRef source)
        {
            Debug.LogWarning("O MANAGER SAIU!");
            Debug.LogWarning("SCENE AUTHORITY:" + NetworkRunnerHandler.runner.IsSceneAuthority);
            Debug.LogWarning("STATE AUTHORITY:" + HasStateAuthority);

            manager = source;


            if (HasStateAuthority)
            {
                _isManager = true;


                Debug.LogWarning(source + " é o novo MANAGER!");
            }
            else
                Debug.LogWarning("Você não tem autoridade pra mudar variaveis");

            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnSwitchManager(source.PlayerId));
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_OnPlayerPause(PlayerRef source, int pause, string thread)
        {
            Debug.LogWarning($"[NetworkedLevelManager] {source} PAUSED:{pause == 1}. IS THREAD:{thread}");
        }


        #endregion

        #region CallBacks


        public override void Spawned()
        {
            base.Spawned();


            _isSpawned = true;


            /* TODO Ver depois
            StartCoroutine(DelayedAction(() =>
            {
                Debug.LogWarning("STATE AUTHORITY:" + HasStateAuthority);
            }, 3f));
             if (currentLandMark >= 0)//It means the Simulation is running
             {
                 HUDManager.Instance.SetElementActive(IsManager);
                // SceneFeature.Instance.TeleportToLandMark(currentLandMark);
             }
            */
        }

        #endregion
    }

}
