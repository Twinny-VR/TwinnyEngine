using Fusion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Twinny.Localization;
using UnityEngine;


namespace Twinny.System.Network
{

    public class ColocationManager : NetworkBehaviour
    {

        private Transform _cameraRigTransform;
        [SerializeField] private AlignmentManager _alignmentManager;

        [Networked] public Vector3 offsetAnchorPosition { get; set; } = default;
        private Vector3 _offsetAnchorPos;  
        [Networked] public float offsetYRotation { get; set; } = 0f;
        private float _offsetYRot;

        private bool _hasSession;

        private OVRSpatialAnchor _anchor;

        private Guid _sharedAnchorGuid;

        private void Start()
        {
          //  OVRColocationSession.ColocationSessionDiscovered += OnColocationSessionDiscovered;
            _cameraRigTransform = FindAnyObjectByType<OVRCameraRig>().transform;
        }

        private void OnDisable()
        {
            //OVRColocationSession.ColocationSessionDiscovered -= OnColocationSessionDiscovered;
            
        }

        public override void Spawned()
        {
            base.Spawned();
            PrepareColocation();
        }

        private void PrepareColocation()
        {
            if (Object.HasStateAuthority)
            {
                RPC_SetOffset( Vector3.zero, 0f);
                Debug.LogWarning("[ColocationManager] Starting advertisement...");
                AdvertisementColocationSession();
            }
            else
            {
                _offsetAnchorPos = offsetAnchorPosition;
                _offsetYRot = offsetYRotation;
                Debug.LogWarning("[ColocationManager] Starting discovery...");
                DiscoveryNearbySession();
            }
        }

        private async void AdvertisementColocationSession()
        {
            /*
            try
            {
                byte[] advertisementData = Encoding.UTF8.GetBytes(s: "SharedSpatialAnchorSession");
                var startAdvertisementResult = await OVRColocationSession.StartAdvertisementAsync(advertisementData);

                if (startAdvertisementResult.Success)
                {
                    _sharedAnchorGuid = startAdvertisementResult.Value;
                    Debug.LogWarning($"[ColocationManager] Advertisement started successfully. UUID: {_sharedAnchorGuid}");
                    CreateAndShareAligmentAnchor();
                }
                else
                {
                    Debug.LogError($"[ColocationManager] Advertisement started failed! Result: {startAdvertisementResult.Value}.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ColocationManager] Error during advertisement: {e.Message}");
            }
            */
        }

        private async void DiscoveryNearbySession()
        {
            /*
            try
            {

                    Debug.LogWarning("[ColocationManager] Seeking for Nearby Session...");
                var discoveryResult = await OVRColocationSession.StartDiscoveryAsync();

                if (!discoveryResult.Success)
                {
                    Debug.LogError($"[ColocationManager] Discovery failed with status: {discoveryResult.Status}.");
                    return;
                }

                    await Task.Delay(3000);
                if (!_hasSession)
                    DiscoveryNearbySession();
                
            }
            catch (Exception e)
            {
                Debug.LogError($"[ColocationManager] Discovery error: {e.Message}");
            }
*/
        }

/*
        private void OnColocationSessionDiscovered(OVRColocationSession.Data session)
        {
            Debug.Log("DESCOBRIU!");
            _hasSession = true;
            _sharedAnchorGuid = session.AdvertisementUuid;
            Debug.LogWarning($"[ColocationManager] Discovered session with UUID: {_sharedAnchorGuid}.");
            LoadAndAlignToAnchor(_sharedAnchorGuid);

        }
*/
        private async void CreateAndShareAligmentAnchor()
        {
            try
            {
                Debug.LogWarning("[ColocationManager] Checkging for existent anchor...");


                if (AnchorManager.currentAnchor == null)
                {

                    Twinny.UI.AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%NO_ANCHOR_MESSAGE"), Twinny.UI.AlertViewHUD.MessageType.Error);

                    while (AnchorManager.currentAnchor == null)
                    {

                        await Task.Yield();
                    }

                    Twinny.UI.AlertViewHUD.CancelMessage();
                }


                Transform anchor = AnchorManager.currentAnchor.transform;

                RPC_SetOffset(anchor.position - _cameraRigTransform.position, anchor.eulerAngles.y);


                Debug.LogWarning("[ColocationManager] Creating alignment anchor...");
                // _anchor = await CreateAnchor(anchor.position, anchor.rotation);
                _anchor = await CreateAnchor(Vector3.zero, Quaternion.identity);

                if (_anchor == null)
                {
                    Debug.LogError("[ColocationManager] Failed to create aligment anchor");
                    return;
                }

                if (!_anchor.Localized)
                {
                    Debug.LogError("[ColocationManager] Anchor is not localized. Cannot proceed with sharing.");
                    return;
                }
                var saveResult = await _anchor.SaveAnchorAsync();

                if (!saveResult.Success)
                {
                    Debug.LogError($"[ColocationManager] Failed to save alignment anchor. Error: {saveResult}.");
                    return;
                }

                Debug.LogWarning($"[ColocationManager] Alignment anchor saved successfully. UUID: {_anchor.Uuid}.");

                /*
                var shareResult = await OVRSpatialAnchor.ShareAsync(anchors: new List<OVRSpatialAnchor> { _anchor }, _sharedAnchorGuid);

                if (!shareResult.Success)
                {
                    Debug.LogError($"[ColocationManager] Failed to share alignment anchor. Error: {shareResult}.");
                    return;
                }
                */
                Debug.LogWarning($"[ColocationManager] Alignment anchor shared successfully. UUID: {_anchor.Uuid}.");
                //  _alignmentManager.AlignUserToAnchor(_anchor);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ColocationManager] Anchor creation and sharing error: {e.Message}.");
            }

        }

        private async Task<OVRSpatialAnchor> CreateAnchor(Vector3 position, Quaternion rotation)
        {

            try
            {
                var anchorGameObject = new GameObject("Alignment Anchor")
                {
                    transform = { position = position, rotation = rotation }
                };

                var spatialAnchor = anchorGameObject.AddComponent<OVRSpatialAnchor>();

                while (!spatialAnchor.Created)
                {
                    await Task.Yield();
                }

                Debug.LogWarning($"[ColocationManager] Anchor created successfully. UUID: {spatialAnchor.Uuid}");
                return spatialAnchor;

            }
            catch (Exception e)
            {

                Debug.LogError($"[ColocationManager] Anchor creation error: {e.Message}.");
                return null;
            }

        }

        public async void LoadAndAlignToAnchor(Guid guid)
        {
            try
            {

                // if(AnchorManager.currentAnchor != null){} TODO disable spatial anchor to receive shared anchors


              //  Debug.LogWarning($"[ColocationManager] Loading anchors for Group UUID: {guid}");

                var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
                /*
                var loadResult = await OVRSpatialAnchor.LoadUnboundSharedAnchorsAsync(guid, unboundAnchors);

                if (!loadResult.Success)
                {

                    Debug.LogError($"[ColocationManager] Failed to load anchors: Success: {loadResult.Success}, Count:{unboundAnchors.Count}.");
                    return;

                }
                */
                foreach (var unboundAnchor in unboundAnchors)
                {
                    if (await unboundAnchor.LocalizeAsync())
                    {

                        Debug.LogWarning($"[ColocationManager] Anchor localized successfully UUID: {unboundAnchor.Uuid}");

                        var anchorGameObject = new GameObject(name: $"Anchor_{unboundAnchor.Uuid}");

                        _anchor = anchorGameObject.AddComponent<OVRSpatialAnchor>();
                        unboundAnchor.BindTo(_anchor);

                        _alignmentManager.AlignUserToAnchor(_anchor, _offsetAnchorPos, _offsetYRot);



                        return;

                    }
                }

            }
            catch (Exception e)
            {
                Debug.LogError($"[ColocationManager] Error during anchor loadind and alignment: {e.Message}.");
            }
        }


        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_SetOffset(Vector3 position, float rotationY)
        {

            offsetAnchorPosition = position;
            offsetYRotation = rotationY;

            Debug.LogWarning($"ANCHOR OFFSET POS: {offsetAnchorPosition} ROT: {offsetYRotation}°.");

        }

    }

}