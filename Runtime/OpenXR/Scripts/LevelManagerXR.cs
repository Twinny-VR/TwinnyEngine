using Concept.Helpers;
using Fusion;
//using Meta.XR.Movement.Networking.Fusion;
using Oculus.Platform;
using Oculus.Platform.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using Twinny.Localization;
using Twinny.System;
using Twinny.System.Network;
using Twinny.UI;
using UnityEngine;

namespace Twinny.XR
{
    public class LevelManagerXR : NetworkedLevelManager
    {
        public static LevelManagerXR instance { get => Instance as LevelManagerXR; }

        public static RuntimeXR Config { get => TwinnyManager.config as RuntimeXR; }

        [SerializeField] private OVRPassthroughLayer _passThrough;
        [SerializeField] private FusionBootstrap _bootstrap;

        private void OnValidate()
        {


            TwinnyManager.LoadRuntimeProfile<RuntimeXR>("RuntimeXRPreset");

        }

        protected override void Awake()
        {
            base.Awake();
            TwinnyManager.LoadRuntimeProfile<RuntimeXR>("RuntimeXRPreset");

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
                UnityEngine.Debug.LogError($"[LevelManager] Unknow Platform initialized ({UnityEngine.Application.platform}).");
            }

            _ = CanvasTransition.FadeScreen(false);


        }

        public async void ConnectToServer()
        {
            //Get Internet Status
            bool isWifiConnected =  NetworkUtils.IsWiFiConnected();


            if (isWifiConnected && !Config.startSinglePlayer)
            {

                try
                {
                    _bootstrap.StartSharedClient();
                }
                catch (global::System.Exception e)
                {
                    Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%ERROR_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Warning, 5);
                    await Task.Delay(4000);
                    await ResetExperience();
                    UnityEngine.Debug.LogError(e.Message);
                }
            await Task.Delay(Config.connectionTimeout * 1000);
            }
            try
            {
                if (NetworkRunnerHandler.runner.IsConnectedToServer) return;
             
                Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%NO_NETWORK_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Warning, 5);
                await Task.Delay(4000);
                _bootstrap.StartSinglePlayer();
                await Task.Delay(Config.connectionTimeout * 1000);

                if (NetworkRunnerHandler.runner.IsConnectedToServer) return;
            }
            catch (global::System.Exception e)
            {
                Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%ERROR_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Warning, 5);
                await Task.Delay(4000);
                await ResetExperience();
                UnityEngine.Debug.LogError(e.Message);
            }

            
            
            Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%ERROR_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Warning, 5);
            await Task.Delay(4000);
            await ResetExperience();

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

        public override async Task ResetExperience()
        {
            await base.ResetExperience();
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
                RenderSettings.skybox = (TwinnyManager.config as RuntimeXR).defaultSkybox;
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
            }

        }
    }

}