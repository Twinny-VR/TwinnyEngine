#if OCULUS && FUSION2
using Fusion;
using Meta.XR.Movement.Networking.Fusion;
using Oculus.Platform;
using Oculus.Platform.Models;
using System.Threading.Tasks;
using Twinny.Helpers;
using Twinny.Localization;
using Twinny.System;
using Twinny.System.Network;
using Twinny.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
namespace Twinny.XR
{
    public class LevelManagerXR : NetworkedLevelManager
    {

        public string maquete;
        public string decorado;

        public static RuntimeXR Config { get { return instance.config as RuntimeXR; } }

        [SerializeField] private FusionBootstrap _bootstrap;
        [SerializeField] private NetworkPoseRetargeterSpawnerFusion _spawner;

#if UNITY_EDITOR
        private void OnValidate()
        {

            config = Resources.Load<RuntimeXR>("RuntimeXRPreset");

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
            //Get Internet Status
            bool isWifiConnected = true;// = NetworkHelper.IsWiFiConnected();


            if (!Config.restarting && isWifiConnected && !Config.startSinglePlayer)
            {
                _bootstrap.StartSharedClient();
                await Task.Delay(Config.connectionTimeout * 1000);

                if (NetworkRunnerHandler.runner.IsConnectedToServer) return;
                else
                {
                   Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%NO_NETWORK_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Warning, 5);
                    Config.restarting = true;
                    await Task.Delay(4000);
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    return;
                }
            }
            Config.restarting = false;
                _bootstrap.StartSinglePlayer();
        }

        public override void GetReady()
        {
            base.GetReady();
            AnchorManager.SpawnColocation();

            // TODO Criar um sistema de spawn caso houver para cada plataforma 
            if (_spawner != null && _spawner.isActiveAndEnabled)
                _spawner.SpawnCharacter();
/*
                NetworkRunnerHandler.runner.Spawn(
                             _playerPrefab,
                             Vector3.zero,
                             Quaternion.identity,
                             NetworkRunnerHandler.runner.LocalPlayer,
                             (runner, obj) => // onBeforeSpawned
                             {
                                 var behaviour = obj.GetComponent<NetworkPoseRetargeterBehaviourFusion>();
                                 behaviour.CharacterId = _spawner.SelectedCharacterIndex + 1;
                             }
                         );

                */

        }



        private void OnLoggedInUser(Message msg)
        {
            if (msg.IsError)
            {
                Debug.LogError("Erro ao verificar informa��es do usu�rio: " + msg.GetError().Message);
                return;
            }

            User user = msg.GetUser();
            var userName = user.DisplayName != "" ? user.DisplayName : user.OculusID;
            Debug.LogWarning("USER:" + userName);
        }

        //TODO Ver utilidade disso
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_AnchorScene()
        {
            if (NetworkRunnerHandler.runner.IsSceneAuthority)
                AnchorManager.AnchorScene();
        }

        [ContextMenu("INICIAR")]
        public void Iniciar()
        {
            StartExperience(maquete, -1); 
        }


        [ContextMenu("VISITAR")]
        public void Visitar()
        {

            RPC_ChangeScene(decorado, 0);
        }


    }
}
#endif