using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Twinny.Helpers;
using Twinny.System;
using Fusion.Sockets;
using System;
using Twinny.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Threading.Tasks;
using Meta.XR.Movement.Networking;
using Meta.XR.Movement.Networking.Fusion;
using Unity.VisualScripting;
using Meta.XR.MultiplayerBlocks.Shared;

namespace Twinny.System.Network
{



    public class NetworkRunnerHandler : TSingleton<NetworkRunnerHandler>, INetworkRunnerCallbacks
    {


        #region Cached Components
        private NetworkRunner _runner;
        public static NetworkRunner runner { get => Instance._runner; }
        #endregion

        [SerializeField] private GameObject _playerPrefab;
        private NetworkObject _playerObject;


        private Transform _cameraTransform;

        #region MonoBehaviour Methods
        private void Awake()
        {
            _runner = GetComponent<NetworkRunner>();
            LevelManager.Instance?.SetOwner(_runner);

        }




        // Start is called before the first frame update
        void Start()
        {
            if (OVRManager.display != null)
                OVRManager.display.RecenteredPose += OnRecenterDetected;

            _cameraTransform = Camera.main.transform;
            Init();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            if (OVRManager.display != null)
                OVRManager.display.RecenteredPose -= OnRecenterDetected;
        }


        #endregion

        #region Private Methods

        private async void SwitchModerator()
        {
            await Task.Delay(100);

            if (_runner.IsSceneAuthority)
            {

                NetworkObject network = LevelManager.Instance.GetComponent<NetworkObject>();

                if (network.HasStateAuthority == false)
                {
                    network.RequestStateAuthority();
                    Debug.LogWarning("Getting State Authority...");
                }


                while (network.HasStateAuthority == false)
                {
                    await Task.Delay(100);
                }

                LevelManager.Instance.RPC_SwitchManager(_runner.LocalPlayer);
                //StartCoroutine(LevelManager.DelayedAction(() =>{}));
            }
            else
                Debug.LogWarning($"[NetworkRunnerHandler] {_runner.LocalPlayer} don't has SceneAuthority");

        }


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


        }

        private void TryReconnect()
        {
            //TODO TryReconnect
        }
        #endregion


        #region Callback Methods


        private void OnRecenterDetected()
        {
            Debug.LogWarning("RECENTER");
            /*
            Debug.LogWarning("MANAGER:" + LevelManager.Instance);
            Debug.LogWarning("CONNECTED:" + runner?.IsConnectedToServer);
            */

            LevelManager.Instance.colocation.SetActive(false);
            LevelManager.Instance.colocation.SetActive(true);
            OVRColocationSession.StartDiscoveryAsync();



            if (LevelManager.Instance == null || (LevelManager.Instance.isRunning && !runner.IsConnectedToServer))
            {
                Debug.LogWarning("[DISCONNECTED] Application is restarting.");

                SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            }
        }

        private void OnApplicationQuit()
        {
            _runner.Shutdown();  // Desconecta a rede e encerra o servidor
            _runner.RemoveCallbacks(this);  // Remover callbacks se você registrou no runner
        }


        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (player == _runner.LocalPlayer)
            {
                LevelManager.Instance.GetReady();
            }

        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {


            Debug.Log($"SAIU:{player} | MODERADOR:{LevelManager.Instance.manager}");

            if (player == LevelManager.Instance.manager)
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
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.LogError($"DISCONECTED: {reason}");
            LevelManager.Instance.RPC_StartForAll(PlayerRef.None, 0);
            if (LevelManager.Instance.tryReconnect)
                TryReconnect();
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
            if (SceneManager.sceneCount > 1)//It means the Simulation is running
            {
                int currentLandMark = LevelManager.Instance.currentLandMark;


                Debug.LogWarning("LOAD DONE| LM:" + LevelManager.Instance.currentLandMark);
                Debug.LogWarning("PLAYERS:" + runner.ActivePlayers.Count());
                Debug.LogWarning("IS MASTER:" + LevelManager.IsManager);
                Debug.LogWarning("MASTER:" + LevelManager.Instance.manager);

                SceneFeature.Instance?.AnchorScene();

                if (LevelManager.Instance?.currentLandMark >= 0)
                {
                    SceneFeature.Instance.TeleportToLandMark(LevelManager.Instance.currentLandMark);
                    if (SceneFeature.Instance.landMarks.Length > 0 && SceneFeature.Instance.currentLandMark != SceneFeature.Instance.landMarks[currentLandMark])
                        SceneFeature.Instance.TeleportToLandMark(currentLandMark);

                }

                StartCoroutine(LevelManager.DelayedAction(() =>
                {
                    HUDManager.Instance.SetElementActive(LevelManager.IsManager ? new string[] { "CONFIG_MENU", SceneFeature.Instance.extensionMenu.name } : null);
                }, 1f));

            }


        }
        public void OnSceneLoadStart(NetworkRunner runner) { }

        #endregion

    }
}