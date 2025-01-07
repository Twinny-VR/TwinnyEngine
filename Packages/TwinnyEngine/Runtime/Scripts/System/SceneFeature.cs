using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Twinny.Helpers;
using Twinny.UI;
using UnityEngine.SceneManagement;

namespace Twinny.System
{
    /// <summary>
    /// Scene layout type VR(Virtual), MR(Mixed), MOBILE(Mobile)
    /// </summary>
    [Serializable]
    public enum SceneType
    {
        VR,//Virtual Reallity
        MR,//Mixed Reallity
        MOBILE //Mobile
    }

    public class SceneFeature : TSingleton<SceneFeature>
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


#if UNITY_EDITOR
        private void OnValidate()
        { 
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

        #region MonoBehaviour Methods   
        //Awake is called before the script is started
        private void Awake()
        {
            _transform = transform;
            Init();
        }

        // Start is called before the first frame update
        void Start()
        {
            if (OVRManager.display != null)
                OVRManager.display.RecenteredPose += OnRecenterDetected;

            if (extensionMenu)
                HUDManager.Instance.LoadExtensionMenu(extensionMenu);

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
            HUDManager.Instance.LoadExtensionMenu(null);
            SetHDRI(-1);

            if (OVRManager.display != null)
                OVRManager.display.RecenteredPose -= OnRecenterDetected;

            if(LevelManager.Instance.currentLandMark < 0 
                && LevelManager.runner.IsConnectedToServer 
                && LevelManager.runner.SessionInfo != null)
            HUDManager.Instance.SetElementActive(new string[] { "MAIN_MENU", "CONFIG_MENU" });


        }

        #endregion

        #region Public Methods


        public void AnchorScene()
        {
            SceneFeature.Instance.transform.position = AnchorManager.Instance.transform.position;
            SceneFeature.Instance.transform.rotation = AnchorManager.Instance.transform.rotation;
#if !UNITY_EDITOR
            gameObject.AddComponent<OVRSpatialAnchor>();
#endif
        }

        /// <summary>
        /// This method change World Transform position to a especific landMark position.
        /// </summary>
        /// <param name="landMarkIndex">Index on landMarks array.</param>
        public void TeleportToLandMark(int landMarkIndex)
        {
            if (landMarks.Length > 0)
            {
                Vector3 localAnchor = transform.position;
                LandMark landMark = landMarks[landMarkIndex];
                worldTransform.localPosition = Vector3.zero;
                worldTransform.localRotation = Quaternion.identity;

                float desiredAngle = transform.eulerAngles.y + landMark.node.transform.eulerAngles.y;

                Vector3 desiredPosition = -landMark.node.transform.localPosition;
                desiredPosition.y = 0;
                worldTransform.localPosition = desiredPosition;
                worldTransform.RotateAround(AnchorManager.Instance.transform.position, Vector3.up, -landMark.node.transform.localRotation.eulerAngles.y);
                SetHDRI(landMarkIndex);
                SetHDRIRotation(worldTransform.localRotation.eulerAngles.y + transform.rotation.eulerAngles.y);
                NavigationMenu.Instance?.SetArrows(enableNavigationMenu ? landMark.node : null);
            }
            else
            {
                worldTransform.position = AnchorManager.Instance.transform.position;
                worldTransform.rotation = AnchorManager.Instance.transform.rotation;
            }
        }


        private void SetHDRI(int landMarkIndex)
        {


            if (landMarks.Length > 0)
            {

                if (landMarkIndex < 0)//If no LandMark to set, reset skybox to Passthroug
                {
                    RenderSettings.skybox = LevelManager.Instance.defaultSkybox;

                    Camera.main.backgroundColor = Color.clear;
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    return;
                }

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
            Camera.main.backgroundColor = Color.clear;
            Camera.main.clearFlags = (sceneType == SceneType.MR) ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;

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


        #endregion


        #region Private Methods

        private void OnRecenterDetected()
        {
            StartCoroutine(RecenterSkyBox());
        }

        private IEnumerator RecenterSkyBox()
        {
            yield return new WaitForEndOfFrame();
            SetHDRIRotation(worldTransform.localRotation.eulerAngles.y + transform.rotation.eulerAngles.y);
        }
        #endregion
    }

}