#if OCULUS && FUSION2
using System.Threading.Tasks;
using Fusion;
using Oculus.Platform;
using Oculus.Platform.Models;
using Twinny.Helpers;
using Twinny.System;
using Twinny.System.Network;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Twinny.XR
{
    public class LevelManagerXR : NetworkedLevelManager
    {

        [SerializeField]
        private FusionBootstrap _bootstrap;

   

        protected override async Task PlatformInitializer()
        {


            await base.PlatformInitializer();

            /* TODO Ver melhor como isso funciona
            if (!Core.IsInitialized()) Core.Initialize();
            Users.GetLoggedInUser().OnComplete(OnLoggedInUser);
            */


            //Get Internet Status
            bool isWifiConnected = NetworkHelper.IsWiFiConnected();


            //Initialize as XR Platform
            if (XRGeneralSettings.Instance && XRGeneralSettings.Instance.InitManagerOnStart)
            {
                Debug.LogWarning("[LevelManager] XR Platform initialized.");
                if (isWifiConnected && !_startSinglePlayer)
                    _bootstrap.StartSharedClient();
                else
                {
                    _bootstrap.StartSinglePlayer();
                }
            }
            else
            {
                Debug.LogError($"[LevelManager] Unknow Platform initialized ({UnityEngine.Application.platform}).");
            }

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

        //TODO Ver utilidade disso
        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_AnchorScene()
        {
            if (NetworkRunnerHandler.runner.IsSceneAuthority)
                AnchorManager.AnchorScene();
        }

    }
}
#endif