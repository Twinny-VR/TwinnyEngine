using Concept.Core;
using Concept.Helpers;
using Fusion;
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Threading.Tasks;
using Twinny.System;
using Twinny.System.Network;
using Twinny.System.XR;
using Twinny.UI;
using UnityEngine;
using static Twinny.System.TwinnyManager;

namespace Twinny.XR
{
    public class LevelManagerXR : NetworkedLevelManager
    {
        public static LevelManagerXR instance { get => Instance as LevelManagerXR; }

        public static RuntimeXR Config { get => config as RuntimeXR; }

        [SerializeField] private Transform m_cameraRigTransform;

        public static Transform cameraRig => instance.m_cameraRigTransform;

        [SerializeField] private OVRPassthroughLayer _passThrough;
        [SerializeField] private FusionBootstrap _bootstrap;
        [SerializeField] private SharedSpatialAnchorManager _sharedAnchorManager;

        #region Delegates
        public delegate void onSetPassthrough(bool status);
        public static onSetPassthrough OnSetPassthrough;
        #endregion

        private void OnValidate()
        {
            LoadRuntimeProfile<RuntimeXR>("RuntimeXRPreset");
        }

        protected override void Awake()
        {
            base.Awake();
            LoadRuntimeProfile<RuntimeXR>("RuntimeXRPreset");

        }


        protected override void Start()
        {
            base.Start();
            if (_sharedAnchorManager == null) _sharedAnchorManager = FindAnyObjectByType<SharedSpatialAnchorManager>();
            OnPlatformInitialize += OnPlatformInitialized;

        }

        private void OnDestroy()
        {
            OnPlatformInitialize -= OnPlatformInitialized;

        }

        private void OnPlatformInitialized(Twinny.System.Platform platform)
        {

            /* TODO Ver melhor como isso funciona
            if (!Core.IsInitialized()) Core.Initialize();
            Users.GetLoggedInUser().OnComplete(OnLoggedInUser);
            */




            //Initialize as XR Platform
            if (platform == Twinny.System.Platform.XR)
            {

                ConnectToServer();
            }
            else
            {
                UnityEngine.Debug.LogError($"[LevelManager] Unknow Platform initialized ({UnityEngine.Application.platform}).");
            }

            _ = CanvasTransition.FadeScreen(false,config.fadeTime);


        }

        public async void ConnectToServer()
        {
            bool isWifiConnected = NetworkUtils.IsWiFiConnected();


            if (isWifiConnected && !Config.startSinglePlayer)
            {
                if (Config.allowNetworkConnections)
                {
                    string ip = await NetworkHelper.GetPublicIP();
                    _bootstrap.DefaultRoomName = ip;
                }
                try
                {
                    _bootstrap.StartSharedClient();

                    // Wait for connection or timeout
                    bool connected = await WaitForConnectionOrTimeout(Config.connectionTimeout);
                    if (connected) return;
                }
                catch (Exception e)
                {
                    //Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%ERROR_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Warning, 5);
                    await Task.Delay(4000);
                    await ResetExperience();
                    UnityEngine.Debug.LogError(e.Message);
                }
            }

            try
            {
                // Connection uncessfully try singleplayer
                // Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%NO_NETWORK_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Warning, 5);
                await Task.Delay(4000);

                _bootstrap.StartSinglePlayer();

                bool connected = await WaitForConnectionOrTimeout(Config.connectionTimeout);
                if (connected)
                {
                    CallbackHub.CallAction<IUIXRCallbacks>(callback => callback.OnConnected(GameMode.Single));
                    return;
                }
            }
            catch (Exception e)
            {
                //Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%ERROR_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Warning, 5);
                await Task.Delay(4000);
                await ResetExperience();
                UnityEngine.Debug.LogError(e.Message);
            }

            // Impossible to connect
            //Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%ERROR_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Warning, 5);
            await Task.Delay(4000);
            await ResetExperience();
        }

        protected override void SetMaster()
        {
            base.SetMaster();
            _sharedAnchorManager.AdvertiseColocationSession();

        }
        private async Task<bool> WaitForConnectionOrTimeout(float timeoutSeconds)
        {
            float elapsed = 0f;
            float checkInterval = 0.1f; // 100ms

            while (elapsed < timeoutSeconds)
            {
                if (NetworkRunnerHandler.runner != null && (NetworkRunnerHandler.runner.IsConnectedToServer || NetworkRunnerHandler.runner.GameMode == GameMode.Single))
                {
                    return true;
                }

                await Task.Delay((int)(checkInterval * 1000));
                elapsed += checkInterval;
            }

            return false; // Connection timeout
        }



        public override void GetReady()
        {
            base.GetReady();
            AnchorManager.SpawnColocation();
        }



        private void OnLoggedInUser(Message msg)
        {
            if (msg.IsError)
            {
                UnityEngine.Debug.LogError("Erro ao verificar informações do usuário: " + msg.GetError().Message);
                return;
            }

            User user = msg.GetUser();
            var userName = user.DisplayName != "" ? user.DisplayName : user.OculusID;
            UnityEngine.Debug.LogWarning("USER:" + userName);
        }

        //TODO Ver utilidade disso tirar rpc daqui de dentro
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_AnchorScene()
        {
            if (NetworkRunnerHandler.runner.IsSceneAuthority)
                AnchorManager.AnchorScene();
        }

#if UNITY_EDITOR
        [ContextMenu("INICIAR")]
        public void Iniciar()
        {
            StartExperience("OculusMockupScene", -1);
        }


        [ContextMenu("VISITAR")]
        public void Visitar()
        {

            RPC_ChangeScene("HallScene", 0);
        }

#endif

        public override async Task QuitExperience()
        {
            await base.QuitExperience();
            await NetworkRunnerHandler.runner.Shutdown(true);
            Shutdown();

        }
        public override async Task ResetExperience()
        {
            await base.ResetExperience();
            await Task.Delay((int)(LevelManagerXR.Config.resetExperienceDelay * 1000));
            await NetworkRunnerHandler.runner.Shutdown(true);
            ResetApplication();
        }

        public override void Shutdown()
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                currentActivity.Call("finish");
            }
        }

        public override void ResetApplication()
        {

            UnityEngine.Debug.LogWarning("[LevelManagerXR] RESET APPLICATION");
            if (UnityEngine.Application.isEditor) return;


            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                const int kIntent_FLAG_ACTIVITY_CLEAR_TASK = 0x00008000;
                const int kIntent_FLAG_ACTIVITY_NEW_TASK = 0x10000000;

                var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                var intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", UnityEngine.Application.identifier);

                intent.Call<AndroidJavaObject>("setFlags", kIntent_FLAG_ACTIVITY_NEW_TASK | kIntent_FLAG_ACTIVITY_CLEAR_TASK);
                currentActivity.Call("startActivity", intent);
                currentActivity.Call("finish");
                var process = new AndroidJavaClass("android.os.Process");
                int pid = process.CallStatic<int>("myPid");
                process.CallStatic("killProcess", pid);
            }
        }
        private AndroidJavaObject GetCurrentActivity()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }

        public void SetPassthrough(bool status)
        {
            UnityEngine.Debug.LogWarning("SetPassthrough: " + status);
            Camera.main.backgroundColor = Color.clear;
            if (status)
            {
                RenderSettings.skybox = (config as RuntimeXR).defaultSkybox;
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
            }
            else
            {
                Camera.main.clearFlags = CameraClearFlags.Skybox;

            }

            if (_passThrough)
            {
                _passThrough.enabled = status;
                _passThrough.gameObject.SetActive(status);

            }
            else
            {
                UnityEngine.Debug.LogWarning("[LevelManagerXR] SetPassthrough was not effective. Cause: 'Passthrough not found'");
                return;
            }
            OnSetPassthrough?.Invoke(status);

        }
    }

}