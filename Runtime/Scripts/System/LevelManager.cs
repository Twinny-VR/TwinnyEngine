#if USING_XR_MANAGEMENT && (USING_XR_SDK_OCULUS || USING_XR_SDK_OPENXR)
#define USING_XR_SDK
#endif

using Fusion;
using Meta.XR.Movement.Networking.Fusion;
using System;
using System.Collections;
using System.Threading.Tasks;
using Twinny.Helpers;
using Twinny.System.Local;
using Twinny.System.Network;
using Twinny.UI;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Twinny.System
{



    public class LevelManager : NSingleton<LevelManager>
    {



        #region Fields
        private NetworkObject _networkObject;
        public static NetworkRunner runner { get => NetworkRunnerHandler.runner; }

        [SerializeField]
        private FusionBootstrap _bootstrap;
        public GameObject colocation;
        private IControls _currentControls;

        [SerializeField]
        private AudioMixer _audioMixer;

        [SerializeField] public static bool IsManager = false;
        [Networked] public PlayerRef manager { get; set; } = PlayerRef.None;


        [Networked] public int currentLandMark { get; set; } = -1;

        public Material defaultSkybox;

        public bool tryReconnect = true;
       public  NetworkPoseRetargeterSpawnerFusion spawner;

        [SerializeField] private GameObject _playerPrefab;
        private NetworkObject _playerObject;

        #endregion

        #region Delegates
        public delegate void onInternetConnectionChanged(bool status);
        public static onInternetConnectionChanged OnInternetConnectionChanged;
        public delegate void onVolumeChanged(float volume);
        public static onVolumeChanged OnVolumeChanged;
        public delegate void onVoipChanged(bool status);
        public static onVoipChanged OnVoipChanged;
        public delegate void onExperienceStatusChange(bool status);
        public onExperienceStatusChange OnExperienceStatusChange;
        #endregion

        public bool isRunning = false;

        /*

        [SerializeField]
        private bool _isRunning;
        public bool isRunning { get => _isRunning; set
            {
                _isRunning = value;

            }
        }
        */
        /*
        [Networked,OnChangedRender(nameof(OnExperienceChange))] public int isRunning { get; set; }
        public bool IsRunning { get => isRunning > 0; set { isRunning = value ? Runner.Tick : -Runner.Tick;} }
        private Tick toggleIsRunningTick => isRunning >= 0 ? isRunning : -isRunning;


        [Networked, OnChangedRender(nameof(OnFadingChange))] public int isFading { get; set; }
        private bool IsFading { get => isFading> 0; set { isFading = value ? Runner.Tick : -Runner.Tick; } }
        private Tick toggleIsFadingTick => isFading>= 0 ? isFading: -isFading;
        */





        #region MonoBehaviour Methods

        //Awake is called before the script is started
        private void Awake()
        {
            Init();
            _networkObject = GetComponent<NetworkObject>();
        }
        // Start is called before the first frame update
        void Start()
        {


            bool isWifiConnected = IsWiFiConnected();
            if (isWifiConnected)
                SetState(new MultiplayerControls());
            else
                SetState(new SinglePlayerControls());

            SetState(new MultiplayerControls()); //TODO Excluir


            StartCoroutine(CheckInternetConnection());


            if (isWifiConnected)
            {
                _bootstrap.StartSharedClient();
            }
            else
            {
                HUDManager.Instance.SetElementActive(new string[] { "CONFIG_MENU" });
                _bootstrap.StartSinglePlayer();
            }


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

        public void SetOwner(NetworkRunner runner)
        {
            if (Instance._networkObject != null)
                Instance._networkObject.SetPlayerAlwaysInterested(runner.LocalPlayer, true);

        }

        [ContextMenu("Start Experience")]
        public void StartExperience() { StartExperience("MockupScene"); }

        public void GetReady()
        {
            isRunning = true;
            //TODO Descobrir se ja esta rodando a experienca
            HUDManager.Instance.SetElementActive(new string[] { "MAIN_MENU", "CONFIG_MENU" });
            runner.Spawn(
                            _playerPrefab,
                            Vector3.zero,
                            Quaternion.identity,
                            runner.LocalPlayer,
                            (runner, obj) => // onBeforeSpawned
                            {
                                var behaviour = obj.GetComponent<NetworkPoseRetargeterBehaviourFusion>();
                                behaviour.CharacterId = spawner.SelectedCharacterIndex + 1;
                            }
                        );

        }

        public void StartExperience(string scene)
        {
            IsManager = true;
            HUDManager.Instance.SetElementActive(new string[] { "CONFIG_MENU" });

            RPC_StartForAll(runner.LocalPlayer, 1);
        }

        [ContextMenu("Quit Experience")]
        public void QuitExperience()
        {
            IsManager = false;
            RPC_StartForAll(PlayerRef.None, 0);
        }




        /// <summary>
        /// This Method return current mixer volume.
        /// </summary>
        /// <param name="mixer">Exposed paramter mixer (MasterVolume is default).</param>
        /// <returns>Current mixer volume.</returns>
        public static float GetAudioVolume(string mixer = "MasterVolume")
        {
            float currentVolume = 0;
            Instance._audioMixer.GetFloat(mixer, out currentVolume);
            return currentVolume;
        }


        /// <summary>
        /// This methos sets the mixer volume value.
        /// </summary>
        /// <param name="volume">Value between -80f(mute) and 0f(normal).</param>
        /// <param name="mixer">Exposed paramter mixer (MasterVolume is default).</param>
        public static void SetAudioVolume(float volume, string mixer = "MasterVolume")
        {
            Instance._audioMixer.SetFloat(mixer, volume);
            LevelManager.OnVolumeChanged?.Invoke(volume);
        }

        /// <summary>
        /// Get primary recorder state
        /// Works only in multiplayer mode
        /// </summary>
        /// <returns>Returns if it's currently transmiting</returns>
        public static bool GetVoipStatus()
        {

            if (Instance._currentControls is MultiplayerControls multiplayer)
                return multiplayer.GetVoipStatus();

            return false;


        }


        /// <summary>
        /// Switch primary recorder transmission
        /// Works only in multiplayer mode
        /// </summary>
        public static void SetVoip()
        {
            if (Instance._currentControls is MultiplayerControls multiplayer)
                multiplayer.SetVoip();
        }

        /// <summary>
        /// Switch Master Mixer volume (Muted/Normal)
        /// </summary>
        public static void SetAudio()
        {
            float currentVolume = GetAudioVolume();
            SetAudioVolume((currentVolume == 0f) ? -80f : 0f);
        }

        /// <summary>
        /// Switch Mixer volume (Muted/Normal)
        /// </summary>
        /// <param name="status">True(Muted)/False(Normal)</param>
        /// <param name="mixer">Exposed paramter mixer.</param>
        public static void MuteAudio(bool status, string mixer)
        {
            SetAudioVolume(status ? -80f : 0f, mixer);
        }

        /// <summary>
        /// This Async Method changes the actual scene.
        /// </summary>
        /// <param name="scene">Scene Name</param>
        /// <param name="landMarkIndex">First LandMark to teleport.</param>
        public async void ChangeScene(string scene, int landMarkIndex)
        {
            HUDManager.Instance.SetElementActive();
            RPC_FadingStatus(0);
            HUDManager.Instance.FadeScreen(false, async (result) =>
            {
                if (scene == "StartScene")
                {
                    await _currentControls.UnloadAdditivesScenes();
                    //RPC_MenuReset(true);
                }
                else
                {
                    await _currentControls.LoadAdditiveSceneAsync(scene, landMarkIndex);
                    //RPC_MenuReset(false);
                }
                HUDManager.Instance.FadeScreen(true);
                RPC_FadingStatus(1);
            });
            await Task.Delay(0);
        }


        /// <summary>
        /// This method is only for handle the Fade Screen during the teleporting
        /// </summary>
        /// <param name="landMarkIndex">LandMark to Teleport</param>
        public void NavigateTo(int landMarkIndex)
        {
            HUDManager.Instance.FadeScreen(false, (result) =>
            {
                SceneFeature.Instance.TeleportToLandMark(landMarkIndex);
                HUDManager.Instance.FadeScreen(true);
            });
        }




        /// <summary>
        /// Checks internet connection
        /// </summary>
        /// <returns>Has Internet: (true or false)</returns>
        public static bool IsWiFiConnected()
        {

            if (Application.platform == RuntimePlatform.Android)
            {

                try
                {
                    // Obter o contexto da UnityActivity
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                        // Acessar o ConnectivityManager do Android
                        using (AndroidJavaClass connectivityManagerClass = new AndroidJavaClass("android.net.ConnectivityManager"))
                        {
                            AndroidJavaObject connectivityManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "connectivity");
                            AndroidJavaObject activeNetwork = connectivityManager.Call<AndroidJavaObject>("getActiveNetworkInfo");

                            if (activeNetwork != null)
                            {
                                bool isConnected = activeNetwork.Call<bool>("isConnected");
                                bool isWiFi = activeNetwork.Call<int>("getType") == 1; // 1 significa Wi-Fi
                                return isConnected && isWiFi;
                            }
                            else
                            {
                                Debug.LogWarning("No active network information available.");
                                return false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Erro ao acessar a conectividade: " + e.Message);
                    return false;
                }
            }
            else
                return UnityEngine.Application.internetReachability != NetworkReachability.NotReachable;
        }
        

        #endregion

        #region Private Methods
        /// <summary>
        /// Switch controlState platform
        /// </summary>
        /// <param name="newState">SinglePlayerControls or MultiPlayerControls</param>
        private void SetState(IControls newState)
        {
            _currentControls = newState;
            _currentControls.SetUp();
        }



        #endregion

        #region Coroutines

        /// <summary>
        /// This coroutine check every a second if has internet connection
        /// </summary>
        /// <param name="callback">Send callback to caller function</param>
        /// <returns></returns>
        private IEnumerator CheckInternetConnection()
        {
            bool conected = IsWiFiConnected();
            while (true)
            {
                //Debug.Log("Teste: "+ conected);
                if (!conected && IsWiFiConnected())
                {
                    conected = true;
                    OnInternetConnectionChanged?.Invoke(true);
                }
                else if (conected && !IsWiFiConnected())
                {
                    conected = false;
                    OnInternetConnectionChanged?.Invoke(false);
                }
                yield return new WaitForSeconds(1);
            }
        }

        //TODO Implementar um Helper
        public static IEnumerator DelayedAction(Action action, float delay = -1)
        {
            if (delay < 0)
                yield return new WaitForEndOfFrame();
            else
                yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        #endregion


        #region RPCs
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_FadingStatus(int status)
        {
            if (!runner.IsSceneAuthority) HUDManager.Instance.FadeScreen(status == 1);
        }


        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_StartForAll(PlayerRef source, int status)
        {
            if (status == 0) currentLandMark = -1;

            manager = source;

            Debug.LogWarning("RPC_StartForAll");

            HUDManager.Instance.SetElementActive(IsManager ? new string[] { "CONFIG_MENU" } : null);


            if (runner.IsSceneAuthority)
                ChangeScene(status == 1 ? "MockupScene" : "StartScene", 0);

#if USING_XR_SDK
            Debug.LogWarning("USING_XR_SDK");
#endif

        }

        [ContextMenu("VISITAR")]
        public void Visitar()
        {

            RPC_ChangeScene("HallScene", 0);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_ChangeScene(string scene, int landMark)
        {
            if (SceneManager.sceneCount > 1)//It means the Simulation is running
            {
                HUDManager.Instance.SetElementActive(IsManager ? new string[] { "CONFIG_MENU" } : null);
                if (runner.IsSceneAuthority)
                    ChangeScene(scene, landMark);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_MenuReset(bool reset)//TODO Substituir
        {
            //HUDManager.Instance.SetElementActive(reset || IsManager);
        }


        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_AnchorScene()
        {
            if (runner.IsSceneAuthority)
                SceneFeature.Instance?.AnchorScene();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_SpawnPlayer(PlayerRef source)//TODO Ver se precisa
        {
            Debug.LogWarning("RPC_SpawnPlayer:" + source);

            if (runner.IsSceneAuthority)
            {
                //SpawnPlayer(source);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_NavigateTo(int landMarkIndex, bool fade = true)
        {
            currentLandMark = landMarkIndex;
            Debug.LogWarning("RPC_NavigateTo");
            if (fade)
                NavigateTo(landMarkIndex);
            else
                SceneFeature.Instance?.TeleportToLandMark(landMarkIndex);

        }


        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_SwitchManager(PlayerRef source)
        {
            Debug.LogWarning("O MANAGER SAIU!");
            Debug.LogWarning("SCENE AUTHORITY:" + runner.IsSceneAuthority);
            Debug.LogWarning("STATE AUTHORITY:" + HasStateAuthority);

            manager = source;


            if (HasStateAuthority)
            {
                IsManager = true;
                string enableMenu = SceneFeature.Instance?.extensionMenu ? SceneFeature.Instance.extensionMenu.name : "MAIN_MENU";
                Debug.LogWarning("OpenMenu:" + enableMenu);
                StartCoroutine(DelayedAction(() =>
                {
                    Debug.LogWarning("MOSTRA MENU!");

                    HUDManager.Instance.SetElementActive(new string[] { enableMenu, "CONFIG_MENU" });
                    if (SceneFeature.Instance.enableNavigationMenu)
                        NavigationMenu.Instance?.SetArrows(SceneFeature.Instance?.landMarks[currentLandMark].node);

                }, 1f));


                Debug.LogWarning(source + " é o novo MANAGER!");
            }
            else
                Debug.LogWarning("Você não tem autoridade pra mudar variaveis");

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