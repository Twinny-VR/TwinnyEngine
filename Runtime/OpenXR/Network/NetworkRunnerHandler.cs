using Fusion;
using System.Collections.Generic;
using UnityEngine;
using Twinny.Helpers;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Threading.Tasks;
using Twinny.Localization;
using Twinny.UI;
using Twinny.XR;
using Concept.Helpers;
using Concept.Core;
using Twinny.System.XR;

namespace Twinny.System.Network
{



    public class NetworkRunnerHandler : TSingleton<NetworkRunnerHandler>, INetworkRunnerCallbacks
    {


        #region Cached Components

        [SerializeField]
        private NetworkRunner _runner;
        public static NetworkRunner runner { get => Instance._runner; }
        #endregion

        #region Delegates
        public delegate void onLoadSceneFeature();
        public static onLoadSceneFeature OnLoadSceneFeature;

        #endregion



        [SerializeField] private GameObject _VoiceRecorder;

        #region MonoBehaviour Methods

        protected override void Awake()
        {
            base.Awake();

            _runner = GetComponent<NetworkRunner>();

        }



        protected override void Start()
        {
            base.Start();
            NetworkedLevelManager.SetOwner(_runner);



            _VoiceRecorder.SetActive(true);

        }




        #endregion

        #region Private Methods

        private async void SwitchModerator()
        {
            await Task.Delay(100);

            if (_runner.IsSceneAuthority)
            {

                NetworkObject network = NetworkedLevelManager.Instance.GetComponent<NetworkObject>();

                if (network.HasStateAuthority == false)
                {
                    network.RequestStateAuthority();
                    Debug.LogWarning("Getting State Authority...");
                }


                while (network.HasStateAuthority == false)
                {
                    await Task.Delay(100);
                }

                NetworkedLevelManager.Instance.RPC_SwitchManager(_runner.LocalPlayer);
                //StartCoroutine(LevelManager.DelayedAction(() =>{}));
            }
            else
                Debug.LogWarning($"[NetworkRunnerHandler] {_runner.LocalPlayer} don't has SceneAuthority");

        }

        /*
        private void SpawnPlayer(PlayerRef player)
        {
            Debug.LogWarning("SpawnPlayer:" + player);

            try
            {
                _playerObject = runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, inputAuthority: player, (runner, obj) => // onBeforeSpawned
                {
                    obj.gameObject.name = player.ToString();
                    var behaviour = obj.GetComponent<NetworkPoseRetargeterBehaviourFusion>();
                    behaviour.CharacterId = UnityEngine.Random.Range(0, 9999);
                });
            }
            catch (Exception)
            {

                Debug.LogWarning("Ignora erro idiota!");
            }


        }*/

        private void TryReconnect()
        {
            //TODO TryReconnect
        }
        #endregion

        #region Public Methods

        #endregion

        #region Callback Methods




        private void OnApplicationQuit()
        {
            _runner.Shutdown();  // Desconecta a rede e encerra o servidor
            _runner.RemoveCallbacks(this);  // Remover callbacks se você registrou no runner
        }


        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            CallbackHub.CallAction<IUIXRCallbacks>(callback => callback.OnPlayerList(runner.ActivePlayers.Count()));

            Debug.LogWarning($"{runner.ActivePlayers.Count()} ONLINE.");
            if (player == _runner.LocalPlayer)
            {
                NetworkedLevelManager.Instance.GetReady();
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            CallbackHub.CallAction<IUIXRCallbacks>(callback => callback.OnPlayerList(runner.ActivePlayers.Count()));

            bool isMaster = player == NetworkedLevelManager.Instance.master;
            Debug.Log($"SAIU:{(isMaster ? "MASTER" : player)}");
            if (isMaster) runner.RequestStateAuthority(LevelManagerXR.instance.Object.Id);
            if (player == NetworkedLevelManager.Instance.manager)
            {
                SwitchModerator();

            }
        }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _runner.RemoveCallbacks(this);
        }
        public void OnConnectedToServer(NetworkRunner runner)
        {
            CallbackHub.CallAction<IUIXRCallbacks>(callback => callback.OnConnected(runner.GameMode));

            Debug.LogWarning($"[NetworkRunnerHandler] *** CONNECTED. {(runner.IsSharedModeMasterClient ? "MASTER" : "USER")}.");
        }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            CallbackHub.CallAction<IUIXRCallbacks>(callback => callback.OnDisconnected());
            Debug.LogError($"DISCONECTED: {reason}");
            //LevelManager.Instance.RPC_StartForAll(PlayerRef.None, "");
            if (LevelManagerXR.Config.tryReconnect)
            {
                Twinny.UI.AlertViewHUD.PostMessage($"{LocalizationProvider.GetTranslated("DISCONNECTED")}!", Twinny.UI.AlertViewHUD.MessageType.Error, 10f);
                TryReconnect();
            }
        }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner)
        {
            if (SceneManager.sceneCount > 2)//It means the Simulation is running
            {
                int currentLandMark = NetworkedLevelManager.Instance.currentLandMark;

                //TODO Remove before build
                Debug.LogWarning($"SCENE LOADED {(currentLandMark == -1 ? "without LandMarks" : $"on LandMark{currentLandMark}")}.");

                if (SceneFeature.Instance != null)
                {
                    OnLoadSceneFeature?.Invoke();
                    if (currentLandMark >= 0)
                    {
                        SceneFeature.Instance.TeleportToLandMark(currentLandMark);
                        //TODO Q diabos fiz aqui
                        /*
                          if (SceneFeatureXR.Instance.landMarks.Length > 0 && SceneFeatureXR.Instance.currentLandMark != SceneFeatureXR.Instance.landMarks[currentLandMark])
                              SceneFeatureXR.Instance.TeleportToLandMark(currentLandMark);
                        */
                    }

                    CallbackHub.CallAction<IUICallBacks>(callback => callback.OnLoadSceneFeature());

                }
                else
                    CallbackHub.CallAction<IUICallBacks>(callback => callback.OnLoadScene());

            }


        }
        public void OnSceneLoadStart(NetworkRunner runner) { }

        #endregion

    }
}
