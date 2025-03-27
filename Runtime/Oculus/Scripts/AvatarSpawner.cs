using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Meta.XR.Movement.Networking.Fusion;
using Twinny.Helpers;
using Twinny.System;
using Twinny.System.Network;
using Twinny.XR;
using UnityEditor;
using UnityEngine;

namespace Twinny
{
    public class AvatarSpawner : TSingleton<AvatarSpawner>
    {

        [SerializeField] private bool _spawnAtStart;

        [SerializeField] private NetworkPoseRetargeterSpawnerFusion _spawner;

        // [SerializeField] private GameObject _playerPrefab;
        private NetworkObject _avatar;
        public static NetworkObject avatar { get => Instance?._avatar; set { Instance._avatar = Instance ? value : null; } }

        [SerializeField] public Transform ikTargetHead;
        [SerializeField] public Transform ikTargetLeftHand;
        [SerializeField] public Transform ikTargetRightHand;


        #region MonoBehaviour Methods

        // Start is called before the first frame update
        void Start()
        {

            OVRManager.HMDLost += () =>
            {
                Debug.LogWarning("[AvatarSpawner] PERDEU");
            };

            OVRManager.HMDAcquired += () =>
            {
                Debug.LogWarning("[AvatarSpawner] RETOMOU");
            };

            OVRManager.HMDMounted += OnHMDMounted;
            OVRManager.InputFocusAcquired += OnHMDMounted;
            OVRManager.HMDUnmounted += OnHMDUnmounted;
            OVRManager.InputFocusLost += OnHMDUnmounted;

            Init();
            if (!_spawner) _spawner = GetComponent<NetworkPoseRetargeterSpawnerFusion>();

            if (_spawnAtStart) SpawnAvatar();

        }



        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            OVRManager.HMDMounted -= OnHMDMounted;
            OVRManager.InputFocusAcquired -= OnHMDMounted;
            OVRManager.HMDUnmounted -= OnHMDUnmounted;
            OVRManager.InputFocusLost -= OnHMDUnmounted;
        }

        /*
        private void OnApplicationFocus(bool focus)
        {
            if(!focus) OnHMDUnmounted();
        }

        private void OnApplicationPause(bool pause)
        {
            int isPaused = pause ? 1 : 0;
            NetworkedLevelManager.instance.RPC_OnPlayerPause(NetworkRunnerHandler.runner.LocalPlayer, isPaused, "NORMAL");

            Thread backgroundThread = new Thread(new ThreadStart(() => {

                Debug.LogWarning("THREAD CALLED!");
                NetworkedLevelManager.instance.RPC_OnPlayerPause(NetworkRunnerHandler.runner.LocalPlayer, isPaused, "THREAD");

            }));
            if(!pause) 
                OnHMDMounted();
        }
        */
        #endregion

        #region Public Methods




        public static async void SpawnAvatar()
        {

            await Task.Yield();
            Debug.LogWarning("[AvatarSpawner] SPAWNING AVATAR");

#if FUSION2 && NETWORK
  
            // TODO Criar um sistema de spawn caso houver para cada plataforma 
            /*
            if (Instance && !Instance._avatar && Instance._spawner != null && Instance._spawner.isActiveAndEnabled)
            {
                Instance._spawner.SpawnCharacter();
                AnchorManager.Recolocation();
            }
            */

           // GameObject prefab = LoadPrefabByGUID("347323ae3c4a7154883956a3769e9e53");

            if(LevelManagerXR.Config == null || LevelManagerXR.Config.avatarPrefab == null)
            {
                Debug.LogWarning("[AvatarSpawner] None avatar to load.");
                return;
            }

            if (Instance && !Instance._avatar)
                NetworkRunnerHandler.runner.Spawn(
                             LevelManagerXR.Config.avatarPrefab,
                             Vector3.zero,
                             Quaternion.identity,
                             NetworkRunnerHandler.runner.LocalPlayer,
                             (runner, obj) => // onBeforeSpawned
                             {
                                 Instance._avatar = obj;
                                 var rig = obj.GetComponent<AvatarRig>();
                                 rig.head.VRTarget = Instance.ikTargetHead;
                                 rig.leftHand.VRTarget = Instance.ikTargetLeftHand;
                                 rig.rightHand.VRTarget = Instance.ikTargetRightHand;
                             }
                         );
#else
            Debug.LogError("[LevelManagerXR] Error impossible to connect without a multiplayer system installed.");
#endif


        }

        public static async void DespawnAvatar()
        {
#if FUSION2 && NETWORK

            Debug.LogWarning($"DESPAWN AVATAR: {avatar}");
            await Task.Yield();
            if (Instance && Instance._avatar)
            {
                NetworkRunnerHandler.runner.Despawn(Instance._avatar);
                Instance._avatar = null;
            }
#else
            Debug.LogError("[LevelManagerXR] Error impossible to connect without a multiplayer system installed.");
#endif

        }

        [ContextMenu("MOUNT")]
        private void OnHMDMounted()
        {
            var feature = SceneFeature.Instance as SceneFeatureXR;
            Debug.LogWarning("[AvatarSpawner] OnHMDMounted event. "+ feature?.sceneType);

            if (feature && feature.sceneType == SceneType.VR)
                SpawnAvatar();
        }


        [ContextMenu("UNMOUNT")]
        private void OnHMDUnmounted()
        {
            Debug.LogWarning("[AvatarSpawner] OnHMDUnmounted event.");
            if (Instance && Instance._avatar)
                DespawnAvatar();

        }
#endregion


        [ContextMenu("SPAWN")]
        public void Spawn()
        {
            SpawnAvatar();
        }
#if UNITY_EDITOR
        /// <summary>
        /// Experimental
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static GameObject LoadPrefabByGUID(string guid)
        {
            // Obtém o caminho do asset usando a GUID
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // Carrega o asset a partir do caminho
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            // Verifica se o prefab foi encontrado
            if (prefab != null)
            {
                Debug.Log($"[AvatarSpawner] Getting prefab by GUID: {guid}");
            }
            else
            {
                Debug.LogWarning($"[AvatarSpawner] Prefab not found by GUID: {guid}");
            }
                return prefab;
        }
#endif

    }
}
