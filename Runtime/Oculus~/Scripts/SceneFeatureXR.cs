using System;
using System.Collections;
using UnityEngine;
using Twinny.Helpers;
using Twinny.UI;
using System.Linq;
using Twinny.System;
using System.Threading.Tasks;
using Fusion;
using Twinny.System.Network;

namespace Twinny.XR
{


    public class SceneFeatureXR : SceneFeature
    {
        #region Cached Components
        private Transform _transform;
        #endregion

        #region Fields
        public Transform worldTransform;
        [SerializeField] public SceneType sceneType;

        [SerializeField] public LandMark[] landMarks = new LandMark[0];
        [SerializeField] public bool enableNavigationMenu;
        [HideInInspector]
        public LandMark currentLandMark;
        public GameObject extensionMenu;
#endregion

        #region Delegates
        public delegate void onTeleportToLandMark(int landMarkIndex);
        public onTeleportToLandMark OnTeleportToLandMark;
        #endregion

        #region MonoBehaviour Methods   


#if UNITY_EDITOR
        private void OnValidate()
        { 
            if (worldTransform == null) worldTransform = new GameObject("World").transform;
            worldTransform.SetParent(transform);
            if (landMarks == null) return;

            foreach (var mark in landMarks)
            {
                if (mark.node != null)
                {
                    mark.landName = mark.node.name;
                }
                else
                    mark.landName = "Empty LandMark";
            }

        }
#endif

        //Awake is called before the script is started
        private void Awake()
        {
            _transform = transform;
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            Init();

            if (OVRManager.display != null)
                OVRManager.display.RecenteredPose += OnRecenterDetected;

            if (extensionMenu)
                CallBackUI.CallAction<IUICallBacks>(callback => callback.OnLoadExtensionMenu(extensionMenu));

            int layer = LayerMask.NameToLayer("Character");

            if (layer == -1) return;


            if(sceneType == SceneType.VR)
            {
                    AvatarSpawner.SpawnAvatar();
                //                Camera.main.cullingMask |= (1 << layer);
            }
            else
            {
              AvatarSpawner.DespawnAvatar();
  //              Camera.main.cullingMask &= ~(1 << layer);

            }


            CheckGameMode();
        }


        // Update is called once per frame
        void Update()
        {

        }

        private void OnDisable()
        {
            NavigationMenu.Instance.SetArrows(null);
        }

        private void OnDestroy()
        {
            SetHDRI(-1);
            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnLoadExtensionMenu(null));

            if (OVRManager.display != null)
                OVRManager.display.RecenteredPose -= OnRecenterDetected;
            if(NetworkedLevelManager.instance.currentLandMark < 0 
                && NetworkRunnerHandler.runner.IsConnectedToServer 
                && NetworkRunnerHandler.runner.SessionInfo != null)
                CallBackUI.CallAction<IUICallBacks>(callback => callback.OnUnloadSceneFeature());

        }

        #endregion

        #region Overrided Methods


        /// <summary>
        /// This method change World Transform position to a especific landMark position.
        /// </summary>
        /// <param name="landMarkIndex">Index on landMarks array.</param>
         public override void TeleportToLandMark(int landMarkIndex)
        {
                SetHDRI(landMarkIndex);
            if (landMarks.Length > 0)
            {
                Vector3 localAnchor = transform.position;
                LandMark landMark = landMarks[landMarkIndex];
                worldTransform.localPosition = Vector3.zero;
                worldTransform.localRotation = Quaternion.identity;

                float desiredAngle = transform.eulerAngles.y + landMark.node.transform.eulerAngles.y;

                Vector3 desiredPosition = -landMark.node.transform.localPosition;
                worldTransform.localPosition = desiredPosition;
                worldTransform.RotateAround(AnchorManager.Instance.transform.position, Vector3.up, -landMark.node.transform.localRotation.eulerAngles.y);
                NavigationMenu.Instance?.SetArrows(enableNavigationMenu ? landMark.node : null);
                SetHDRIRotation(worldTransform.localRotation.eulerAngles.y + transform.rotation.eulerAngles.y);
            }
            else
            {
                worldTransform.position = AnchorManager.Instance.transform.position;
                worldTransform.rotation = AnchorManager.Instance.transform.rotation;
            }

            OnTeleportToLandMark?.Invoke(landMarkIndex);
        }

        #endregion

        #region Public Methods


        public LandMark GetLandMark(LandMarkNode node)
        {
            return landMarks.FirstOrDefault(o => o.node == node);
        }

        public int GetLandMarkIndex(LandMarkNode node)
        {
            for (int i = 0; i < landMarks.Length; i++)
            {
                if (landMarks[i].node == node)
                { return i; }
            }
            return -1;
        }

        public void OnRecenterDetected()
        {
            _ = RecenterSkyBox();
        }


        #endregion

        #region Private Methods


        private void SetHDRI(int landMarkIndex)
        {

            //TODO Melhorar o sistema de HDRI
            //TODO Arrumar ao troca de cena


                if (landMarkIndex < 0)//If no LandMark to set, reset skybox to Passthroug
                {
                LevelManagerXR.Instance.SetPassthrough(true);
                    return;
                }

            if (landMarks.Length > 0)
            {

                LandMark landMark = landMarks[landMarkIndex];
                if (landMark.skyBoxMaterial)
                {

                    if (RenderSettings.skybox != landMark.skyBoxMaterial)
                    {
                        RenderSettings.skybox = landMark.skyBoxMaterial;
                        DynamicGI.UpdateEnvironment();
                    }
                }
                else
                    Debug.LogWarning("[SceneFeature] Warning! The Skybox Material has not been defined.");
                currentLandMark = landMark;
            }

            LevelManagerXR.Instance.SetPassthrough(sceneType == SceneType.MR);

        }

        private void SetHDRIRotation(float angle)
        {
            if (!RenderSettings.skybox) { Debug.LogWarning("[SceneFeature] Warning! The Skybox Material has not been defined."); return; }

            angle -= currentLandMark.hdriOffsetRotation;

            angle = angle % 360;

            if (angle < 0)
            {
                angle += 360;
            }

            float rotationOffset = 0;

            if (angle > 0)
                rotationOffset = 360f - angle;
            else
                rotationOffset = angle + 360;


            rotationOffset = Mathf.Clamp(rotationOffset, 0, 360);

            RenderSettings.skybox.SetFloat("_Rotation", rotationOffset);
        }

        private void CheckGameMode()
        {

            bool active = (NetworkRunnerHandler.runner.GameMode != Fusion.GameMode.Single);
                NetworkTransform[] networks = _transform.GetComponentsInChildren<NetworkTransform>();
                foreach (var item in networks)
                {
                    item.enabled = active;
                }
        }
        private async Task RecenterSkyBox()
        {
            await Task.Yield();
            Debug.Log("[SceneFeatureXR] RecenterSkyBox: " + worldTransform);
            SetHDRIRotation(worldTransform.localRotation.eulerAngles.y + transform.rotation.eulerAngles.y);
        }

        #endregion
    }

}
