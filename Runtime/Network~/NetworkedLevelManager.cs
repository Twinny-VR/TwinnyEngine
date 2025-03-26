#if FUSION2
using Fusion;
using System.Threading.Tasks;
using Twinny.Helpers;
using Twinny.Localization;
using Twinny.System.Network;
using Twinny.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Twinny.System
{


    [RequireComponent(typeof(NetworkObject))]
    public abstract class NetworkedLevelManager : NetworkBehaviour
    {

        #region Singleton Instance
        private static NetworkedLevelManager _instance;
        public static NetworkedLevelManager instance { get { return _instance; } }
        #endregion

        #region Fields

        [SerializeField] public TwinnyRuntime config;// { get; set; }
        
        protected NetworkObject _networkObject;

        [Space]
        [Header("Linked Components")]


        [SerializeField] protected bool _isManager = false;
        public static bool IsManager { get => instance._isManager; }
        [Networked] public PlayerRef manager { get; set; } = PlayerRef.None;

        [Networked] public int currentLandMark { get; set; } = -1;



        #endregion

        #region Delegates
        #endregion

        public static bool isRunning = false;

        #region MonoBehaviour Methods

        //Awake is called before the script is started
        protected virtual void Awake()
        {
            if(_instance == null)
                _instance = this;
            else
            {
                Destroy(_instance);
                Debug.LogWarning("[LevelManager] Multiple instance removed.");
            }
                
            _networkObject = GetComponent<NetworkObject>();
        }
        // Start is called before the first frame update
        protected virtual void Start()
        {
            _ = TwinnyManager.InitializePlatform();
        }

        // Update is called once per frame
        void Update()
        {

        }
        // Destroy is called when component/object was removed
        private void OnDestroy()
        {
        }



        #endregion

        #region Public Methods

        public static void SetOwner(NetworkRunner runner)
        {
            if (instance._networkObject != null)
                instance._networkObject.SetPlayerAlwaysInterested(runner.LocalPlayer, true);

        }


        public virtual void GetReady()
        {
            isRunning = true;

            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnExperienceReady());
        }

        public static void StartExperience(string scene, int landMarkIndex)
        {
            instance._isManager = true;



            instance.RPC_StartForAll(NetworkRunnerHandler.runner.LocalPlayer, scene, landMarkIndex);
        }

        [ContextMenu("Quit Experience")]
        public static void QuitExperience()
        {
            instance._isManager = false;
            instance.RPC_StartForAll(PlayerRef.None, "PlatformScene");
        }

        public virtual async Task ResetExperience()
        {
            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnExperienceFinished(false));

            await CanvasTransition.FadeScreen(true);

            SceneManager.LoadScene(0);
        }
        public virtual void ResetApplication()
        {
            Debug.LogWarning("[NetworkedLevelManager] RESET APPLICATION");
        }

        /// <summary>
        /// This Async Method changes the actual scene.
        /// </summary>
        /// <param name="scene">Scene Name</param>
        /// <param name="landMarkIndex">First LandMark to teleport.</param>
        public async Task ChangeScene(string scene, int landMarkIndex)
        {
            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnStartLoadScene());

            RPC_FadingStatus(1);
            RPC_Message(Runner.LocalPlayer, PlayerRef.None, LocalizationProvider.GetTranslated("%LOADING_SCENE"), time: 90f);

            await CanvasTransition.FadeScreen(true);

            //TODO Mudar o sistema de carregamento de cenas
            if (scene == "PlatformScene")
            {
                await NetworkSceneManager.UnloadAdditivesScenes();
                CallBackUI.CallAction<IUICallBacks>(callback => callback.OnExperienceFinished(isRunning));
            }
            else
            {
                await NetworkSceneManager.LoadAdditiveSceneAsync(scene, landMarkIndex);

            }

            RPC_FadingStatus(0);
            RPC_Message(Runner.LocalPlayer, PlayerRef.None, "");

            await CanvasTransition.FadeScreen(false);

            

        }


        /// <summary>
        /// This method is only for handle the Fade Screen during the teleporting
        /// </summary>
        /// <param name="landMarkIndex">LandMark to Teleport</param>
        public async void NavigateTo(int landMarkIndex)
        {
            await CanvasTransition.FadeScreen(true);
            SceneFeature.Instance.TeleportToLandMark(landMarkIndex);
            await CanvasTransition.FadeScreen(false);
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
            if (!NetworkRunnerHandler.runner.IsSceneAuthority) _ = CanvasTransition.FadeScreen(status == 1);
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

            if (source == NetworkRunnerHandler.runner.LocalPlayer) _isManager = true;

            Debug.LogWarning(IsManager ? "YOU ARE THE MASTER!" : (source != PlayerRef.None ? $"{source} IS THE MASTER!" : "NO MASTERS"));

            if (source != PlayerRef.None)
                CallBackUI.CallAction<IUICallBacks>(callback => callback.OnExperienceStarting());


            if (NetworkRunnerHandler.runner.IsSceneAuthority)
                _ = ChangeScene(scene, landMark);

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

            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnSwitchManager(source.PlayerId));
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_OnPlayerPause(PlayerRef source, int pause, string thread)
        {
            Debug.LogWarning($"[NetworkedLevelManager] {source} PAUSED:{pause==1}. IS THREAD:{thread}");
        }


        #endregion

        #region CallBacks


        public override void Spawned()
        {
            base.Spawned();

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
#endif