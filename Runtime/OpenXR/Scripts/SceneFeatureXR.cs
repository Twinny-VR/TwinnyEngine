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
using Concept.Core;
using UnityEngine.SceneManagement;

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
        public GameObject extensionMenu;
        public bool isMenuStatic;
        private LandMark currentLandMark;
        private LandMark _currentLandMark;
        #endregion

        #region Delegates
        public delegate void onTeleportToLandMark(int landMarkIndex);
        public onTeleportToLandMark OnTeleportToLandMark;
        #endregion

        #region MonoBehaviour Methods   


#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
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
        protected override void Awake()
        {
            base.Awake();
            _transform = transform;
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

            if (fadeOnAwake)
            {
                NetworkedLevelManager.Instance.RPC_FadingStatus(0);
                _ = CanvasTransition.FadeScreen(false, TwinnyRuntime.GetInstance<RuntimeXR>().fadeTime);
            }



            
            if (OVRManager.display != null)
                OVRManager.display.RecenteredPose += OnRecenterDetected;

            if (extensionMenu)
                CallbackHub.CallAction<IUICallBacks>(callback => callback.OnLoadExtensionMenu(extensionMenu, isMenuStatic));

            int layer = LayerMask.NameToLayer("Character");

            if (layer == -1) return;


            if (sceneType == SceneType.VR)
            {
                // AvatarSpawner.SpawnAvatar();
                //                Camera.main.cullingMask |= (1 << layer);
            }
            else
            {
                //AvatarSpawner.DespawnAvatar();
                //              Camera.main.cullingMask &= ~(1 << layer);

            }


            CheckGameMode();
        }

        private void OnDisable()
        {
            NavigationMenu.Instance.SetArrows(null);
        }

        private void OnDestroy()
        {
            SetHDRI(-1);
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnLoadExtensionMenu(null));

            if (OVRManager.display != null)
                OVRManager.display.RecenteredPose -= OnRecenterDetected;

            if (NetworkedLevelManager.Instance.currentLandMark < 0
                && NetworkRunnerHandler.runner.IsConnectedToServer
                && NetworkRunnerHandler.runner.SessionInfo != null)
                CallbackHub.CallAction<IUICallBacks>(callback => callback.OnUnloadSceneFeature());
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
                if (_currentLandMark != null) _currentLandMark.node?.OnLandMarkUnselected?.Invoke();
                _currentLandMark = landMarks[landMarkIndex];

                worldTransform.localPosition = Vector3.zero;
                worldTransform.localRotation = Quaternion.identity;

                float desiredAngle = transform.eulerAngles.y + _currentLandMark.node.transform.eulerAngles.y;

                Vector3 desiredPosition = -_currentLandMark.node.transform.localPosition;
                worldTransform.localPosition = desiredPosition;
                worldTransform.RotateAround(AnchorManager.Instance.transform.position, Vector3.up, -_currentLandMark.node.transform.localRotation.eulerAngles.y);
               
                NavigationMenu.Instance?.SetArrows(enableNavigationMenu ? _currentLandMark.node : null);
                
                SetHDRIRotation(worldTransform.localRotation.eulerAngles.y + transform.rotation.eulerAngles.y);

                _currentLandMark.node?.OnLandMarkSelected?.Invoke();

                Transform cameraRig = LevelManagerXR.cameraRig;
                bool turnParent = _currentLandMark.node.changeParent;


                if (turnParent)
                    cameraRig.SetParent(_currentLandMark.node.newParent);
                else
                {
                    cameraRig.SetParent(null);
                    cameraRig.position = Vector3.zero;
                    SceneManager.MoveGameObjectToScene(cameraRig.gameObject, SceneManager.GetActiveScene());
                }
            }
            else
            {
                _currentLandMark = null;
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
                LevelManagerXR.instance.SetPassthrough(true);
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

            LevelManagerXR.instance.SetPassthrough(sceneType == SceneType.MR);

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
