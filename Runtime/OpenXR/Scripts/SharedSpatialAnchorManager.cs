using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Concept.Helpers;
using Fusion;
using Twinny.Helpers;
using Twinny.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Twinny.System.XR
{
    public class SharedSpatialAnchorManager : NetworkBehaviour
    {
        [SerializeField] private AnchorDebug _anchorDebug;

        private ColocationManager _colocationManager;
        private Guid _sharedGuid;
        private Transform _cameraRigTransform;
        private OVRSpatialAnchor _sharedAnchor;
        public OVRSpatialAnchor sharedAnchor { get => _sharedAnchor; }
        private void Awake()
        {
            _cameraRigTransform = FindAnyObjectByType<OVRCameraRig>()?.transform;
        }

        public override void Spawned()
        {

            base.Spawned();
            _colocationManager = FindAnyObjectByType<ColocationManager>();
            InitializeColocation();
        }

        private void InitializeColocation()
        {
            if (AnchorManager.currentAnchor == null) {
                DebugPanel.Debug("<color=#00FF00>LOCAL ANCHOR NOT FOUND!"," ", UnityEngine.LogType.Warning);
                if (Object.HasStateAuthority) return;//TODO Wait for anchor system
            }
            if (Object.HasStateAuthority)
                AdvertiseColocationSession();
            else
                DiscoverNearbySession();
        }

        public async void AdvertiseColocationSession()
        {
          //  if (AnchorManager.currentAnchor == null) return; //TODO Create a waiting for an anchor system
            Debug.Log("[SharedSpatialAnchorManager] Starting advertisement...");
            byte[] advertisementData = Encoding.UTF8.GetBytes(s: "SharedSpatialAnchorSession");
            var result = await OVRColocationSession.StartAdvertisementAsync(advertisementData);

            if (result.Success)
            {
                _sharedGuid = result.Value;
                Debug.Log($"[SharedSpatialAnchorManager] Advertisement started sucessfully. UUID: {_sharedGuid}");
                CreateAndShareAlignmentAnchor();
            }
            else
            {
                Debug.Log($"[SharedSpatialAnchorManager] Advertisement failed! Status: {result.Status}");
                return;
            }
        }

        private async void DiscoverNearbySession()
        {
            Debug.Log("[SharedSpatialAnchorManager] Starting discovery session...");

            OVRColocationSession.ColocationSessionDiscovered += OnColocationSessionDiscovered;

            var result = await OVRColocationSession.StartDiscoveryAsync();

            if (result.Success)
                Debug.Log("[SharedSpatialAnchorManager] Discovery session started successfully.");
            else
            {
                Debug.LogError($"[SharedSpatialAnchorManager] Discovery session failed! Status: {result.Status}");
                return;
            }

        }

        private void OnColocationSessionDiscovered(OVRColocationSession.Data data)
        {
            OVRColocationSession.ColocationSessionDiscovered -= OnColocationSessionDiscovered;
            Debug.Log($"[SharedSpatialAnchorManager] Discovery session with UUID: {_sharedGuid}");
            LoadAndAlignToAnchor(_sharedGuid);
        }

        private async void CreateAndShareAlignmentAnchor()
        {
            Debug.Log("[SharedSpatialAnchorManager] Creating alignment anchor...");
            Transform anchorTransform = AnchorManager.Instance.transform;
            //_sharedAnchor = await CreateAnchor(anchorTransform.position, anchorTransform.rotation);
            _sharedAnchor = await CreateAnchor();

            if (_sharedAnchor == null) Debug.LogError("[SharedSpatialAnchorManager] Failed to create Alignment Anchor.");
            else
            if (!_sharedAnchor.Localized) Debug.LogError("[SharedSpatialAnchorManager] No Anchor localized to sharing.");
            else
            {
                var saveResult = await _sharedAnchor.SaveAnchorAsync();
                if (saveResult.Success)
                {
                    Debug.Log($"[SharedSpatialAnchorManager] Alignment anchor saved successfully. UUID: {_sharedAnchor.Uuid}");

                    Debug.Log("[SharedSpatialAnchorManager] Attempting to share alignment anchor...");
                    var shareResult = await OVRSpatialAnchor.ShareAsync(new List<OVRSpatialAnchor> { _sharedAnchor }, _sharedGuid);

                    if (shareResult.Success)
                        Debug.Log($"[SharedSpatialAnchorManager] Alignment anchor shared successfully. Group UUID: {_sharedGuid}");
                    else
                    {
                        Debug.LogError($"[SharedSpatialAnchorManager] Failed to share Alignment Anchor. {shareResult}");
                    }
                }
                else
                {
                    Debug.LogError($"[SharedSpatialAnchorManager] Failed to save Alignment Anchor. {saveResult}");
                }
            }

        }

        private async Task<OVRSpatialAnchor> CreateAnchor(Vector3? position = default, Quaternion? rotation = default)
        {


            var desiredPosition = position ?? Vector3.zero;
            desiredPosition.y = 0;
            var desiredRotation = rotation ?? Quaternion.identity;


            var anchorDebug = Instantiate(_anchorDebug,desiredPosition,desiredRotation);
            anchorDebug.displayText = "[Twinny] Alignment Anchor";
            anchorDebug.showInfo = true;
            var spatialAnchor = anchorDebug.gameObject.AddComponent<OVRSpatialAnchor>();
            while (!spatialAnchor.Created)
            {
                await Task.Yield();
            }

            Debug.Log($"[SharedSpatialAnchorManager] Anchor created successfully. UUID: {spatialAnchor.Uuid}");

            return spatialAnchor;
        }

        private async void LoadAndAlignToAnchor(Guid guid)
        {
            Debug.Log($"[SharedSpatialAnchorManager] Loading anchors for Group UUID: {guid}...");

            var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
            var result = await OVRSpatialAnchor.LoadUnboundSharedAnchorsAsync(guid, unboundAnchors);

            if (!result.Success) Debug.LogError("[SharedSpatialAnchorManager] Failed to load anchors!");
            else
            if (unboundAnchors.Count == 0) Debug.LogError("[SharedSpatialAnchorManager] Anchors not found.");
            else
            {
                foreach (var anchor in unboundAnchors)
                {
                    if (await anchor.LocalizeAsync())
                    {
                        Debug.Log($"[SharedSpatialAnchorManager] Anchor localized successfully. UUID: {anchor.Uuid}");

                        var anchorDebug = Instantiate(_anchorDebug);
                        anchorDebug.displayText = $"Anchor_{anchor.Uuid}";
                        anchorDebug.showInfo = true;
                        _sharedAnchor = anchorDebug.gameObject.AddComponent<OVRSpatialAnchor>();
                        anchor.BindTo(_sharedAnchor);

                        await sharedAnchor.SaveAnchorAsync();
                        AlignUserToAnchor(_sharedAnchor);
                       // AnchorManager.PlaceSafeArea(_sharedAnchor);

                        // RPC_GetSafeArea();
                        return;
                    }

                    Debug.LogError($"[SharedSpatialAnchorManager] Failed to localize anchor! UUID: {anchor.Uuid}");
                }
            }
        }

        public void AlignUserToAnchor(OVRSpatialAnchor anchor)
        {
            if (!anchor || !anchor.Localized)
            {
                Debug.LogError("[SharedSpatialAnchorManager] Invalid or un-localized anchor. Cannot align.");
                return;
            }

            if (_cameraRigTransform == null)
            {
                Debug.LogError("[SharedSpatialAnchorManager] OVRCameraRig not found.");
                return;
            }

            _cameraRigTransform.position = anchor.transform.InverseTransformPoint(default);
            _cameraRigTransform.eulerAngles = new Vector3(0, -anchor.transform.eulerAngles.y, 0);
            Debug.LogWarning("[SharedSpatialAnchorManager] Alignment Complete!");
        }

        /*

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_GetSafeArea(RpcInfo info = default)
        {
            Debug.Log("RPC_GetSafeArea: " + info.Source);

            if (AnchorManager.Instance == null)
            {
                Debug.LogError("[SharedSpatialAnchorManager] Instace of AnchorManager not found!");
                return;
            }

            Transform safeArea = AnchorManager.Instance.transform;
            if (HasStateAuthority) RPC_PlaceSafeArea( safeArea.position, safeArea.rotation);

            Debug.LogWarning($"Enviando SafeArea POS: {safeArea.position} | ROT: {safeArea.rotation}");
        }


        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_PlaceSafeArea(Vector3 position, Quaternion rotation, RpcInfo info = default)
        {
            Debug.Log("RPC_PlaceSafeArea: " + info.Source);

            if (!HasStateAuthority)
            {
            Debug.LogWarning($"Recebendo SafeArea POS: {position} | ROT: {rotation}");
            if (_sharedAnchor != null) AnchorManager.PlaceSafeArea(_sharedAnchor, position, rotation);
            }
        }
        */

    }
}
