using Fusion;
using Meta.XR.Movement.Networking.Fusion;
using Oculus.Platform;
using Oculus.Platform.Models;
using System.Threading.Tasks;
using Twinny.Localization;
using Twinny.System;
using Twinny.System.Network;
using Twinny.UI;
using UnityEngine;

namespace Twinny.XR
{
#if NETWORK 
    public class LevelManagerXR : NetworkedLevelManager
#else
    public class LevelManagerXR : LevelManager
#endif
    {
        public static LevelManagerXR instance { get => Instance as LevelManagerXR; }


        public static RuntimeXR Config { get { return Instance.config as RuntimeXR; } }

        [SerializeField] private OVRPassthroughLayer _passThrough;
        [SerializeField] private FusionBootstrap _bootstrap;

#if UNITY_EDITOR
        private void OnValidate()
        {

                #if NETWORK

            config = Resources.Load<RuntimeXR>("RuntimeXRPreset");
#else
            config = Resources.Load<RuntimeXR>("RuntimeXRPreset");
#endif
            if (config == null)
            {
                Debug.LogError("[LevelManagerXR] Impossible to load 'RuntimeXRPreset'.");
            }

        }
#endif

        protected override void Awake()
        {
            base.Awake();
            config = Resources.Load<RuntimeXR>("RuntimeXRPreset");

            if (config == null)
            {
                Debug.LogError("[NetworkedLevelManager] Impossible to load 'RuntimeXRPreset'.");
            }

        }


        protected override void Start()
        {
            base.Start();
            TwinnyManager.OnPlatformInitialize += OnPlatformInitialized;
        }

        private void OnDestroy()
        {
            TwinnyManager.OnPlatformInitialize -= OnPlatformInitialized;

        }

        private void OnPlatformInitialized(Platform platform)
        {

            /* TODO Ver melhor como isso funciona
            if (!Core.IsInitialized()) Core.Initialize();
            Users.GetLoggedInUser().OnComplete(OnLoggedInUser);
            */




            //Initialize as XR Platform
            if (platform == Platform.XR)
            {

                ConnectToServer();
            }
            else
            {
                Debug.LogError($"[LevelManager] Unknow Platform initialized ({UnityEngine.Application.platform}).");
            }

            _ = CanvasTransition.FadeScreen(false);


        }

        public async void ConnectToServer()
        {
#if NETWORK
            //Get Internet Status
            bool isWifiConnected = true;// = NetworkHelper.IsWiFiConnected();


            if (!Config.restarting && isWifiConnected && !Config.startSinglePlayer)
            {

                try
                {
                    _bootstrap.StartSharedClient();
                }
                catch (global::System.Exception e)
                {
                    Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%ERROR_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Warning, 5);
                    await Task.Delay(4000);
                    Config.restarting = true;
                    await ResetExperience();
                    Debug.LogError(e.Message);
                }

                await Task.Delay(Config.connectionTimeout * 1000);

                if (NetworkRunnerHandler.runner.IsConnectedToServer) return;
                else
                {
                    Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%NO_NETWORK_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Warning, 5);
                    await Task.Delay(4000);
                    Config.restarting = true;
                    await ResetExperience();
                    return;
                }
            }
            Config.restarting = false;
            try
            {
                _bootstrap.StartSinglePlayer();
            }
            catch (global::System.Exception e)
            {
                Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%ERROR_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Warning, 5);
                await Task.Delay(4000);
                Config.restarting = true;
                await ResetExperience();
                Debug.LogError(e.Message);
            }
#else
            Debug.LogError("[LevelManagerXR] Error impossible to connect without a multiplayer system installed.");
#endif

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
                Debug.LogError("Erro ao verificar informações do usuário: " + msg.GetError().Message);
                return;
            }

            User user = msg.GetUser();
            var userName = user.DisplayName != "" ? user.DisplayName : user.OculusID;
            Debug.LogWarning("USER:" + userName);
        }

#if FUSION2 && NETWORK
        //TODO Ver utilidade disso tirar rpc daqui de dentro
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_AnchorScene()
        {
            if (NetworkRunnerHandler.runner.IsSceneAuthority)
                AnchorManager.AnchorScene();
        }
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
        public override async Task ResetExperience()
        {
            await base.ResetExperience();
        }

        public override void ResetApplication()
        {

            Debug.LogWarning("[LevelManagerXR] RESET APPLICATION");
            if (UnityEngine.Application.isEditor) return;


            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                AndroidJavaObject pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                AndroidJavaObject intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", UnityEngine.Application.identifier);
                intent.Call<AndroidJavaObject>("setFlags", 0x20000000);//Intent.FLAG_ACTIVITY_SINGLE_TOP

                AndroidJavaClass pendingIntent = new AndroidJavaClass("android.app.PendingIntent");
                AndroidJavaObject contentIntent = pendingIntent.CallStatic<AndroidJavaObject>("getActivity", currentActivity, 0, intent, 0x8000000); //PendingIntent.FLAG_UPDATE_CURRENT = 134217728 [0x8000000]
                AndroidJavaObject alarmManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "alarm");
                AndroidJavaClass system = new AndroidJavaClass("java.lang.System");
                long currentTime = system.CallStatic<long>("currentTimeMillis");
                alarmManager.Call("set", 1, currentTime + 1000, contentIntent); // android.app.AlarmManager.RTC = 1 [0x1]

                Debug.LogError("alarm_manager set time " + currentTime + 1000);
                currentActivity.Call("finish");

                AndroidJavaClass process = new AndroidJavaClass("android.os.Process");
                int pid = process.CallStatic<int>("myPid");
                process.CallStatic("killProcess", pid);
            }

        }

  
    public void SetPassthrough(bool status)
        {
            Debug.LogWarning("SetPassthrough: " + status);
            Camera.main.backgroundColor = Color.clear;
            if (status)
            {
                RenderSettings.skybox = instance.config.defaultSkybox;
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

            }else {
                Debug.LogWarning("[LevelManagerXR] SetPassthrough was not effective. Cause: 'Passthrough not found'");
                    }

        }
    }

}