using Meta.XR.BuildingBlocks;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using OVR.OpenVR;
using System;
using System.Collections;
using System.Collections.Generic;

using Twinny.Helpers;
using Twinny.UI;
using Unity.VisualScripting;
using UnityEngine;
using static OVRPlugin;


namespace Twinny.System
{


    public enum StateAnchorManager
    {
        DISABLED,
        ANCHORED,
        ANCHORING
    }

    /// <summary>
    /// This class handles the Anchors Management
    /// </summary>
    public class AnchorManager : TSingleton<AnchorManager>
    {
        #region Cached Components
        private Transform _transform;
        #endregion

        #region Fields
        [SerializeField] private SpatialAnchorCoreBuildingBlock _spatialAnchorCore;
        [SerializeField] private SpatialAnchorLoaderBuildingBlock _spatialAnchorLoader;
        [SerializeField] private SpatialAnchorSpawnerBuildingBlock _spatialAnchorSpawner;

        [SerializeField] private float _maxDistance = 5f;

        public delegate void onAnchorStateChanged(StateAnchorManager state);
        public static onAnchorStateChanged OnAnchorStateChanged;

        private StateAnchorManager _stateAnchorManager = StateAnchorManager.DISABLED;
        private StateAnchorManager _state {  get { return _stateAnchorManager; } set {

                OnAnchorStateChanged?.Invoke(value);

                _stateAnchorManager = value;
                bool setActive = value == StateAnchorManager.DISABLED;

                string enableMenu = (SceneFeature.Instance?.extensionMenu ? SceneFeature.Instance.extensionMenu.name : "MAIN_MENU");
                
                HUDManager.Instance.SetElementActive(setActive?new string[] { enableMenu, "CONFIG_MENU"}:new string[] { "CONFIG_MENU" });

                if(value == StateAnchorManager.ANCHORING)
                {
                    OVRSpatialAnchor currentAnchor;
                    TryGetComponent<OVRSpatialAnchor>(out currentAnchor);

                    if (currentAnchor != null)
                        Destroy(GetComponent<OVRSpatialAnchor>());

                    _spatialAnchorCore.EraseAllAnchors();
                }
     

            } }
        #endregion

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

            //Set callbacks listeners
            _spatialAnchorCore.OnAnchorsLoadCompleted.AddListener(OnAnchorsLoadCompleted);
            _spatialAnchorCore.OnAnchorCreateCompleted.AddListener(OnAnchorCreateCompleted);

            //Set callbacks delegates
            GestureMonitor.Instance.OnPinchLeft += OnPinchLeft;
            GestureMonitor.Instance.OnPinchRight += OnPinchRight;

            _spatialAnchorLoader.LoadAnchorsFromDefaultLocalStorage();
        }

        // Update is called once per frame
        void Update()
        {
            //Anchor placement system
            if (_state != StateAnchorManager.ANCHORING) return;
            Handling();
        }

        //OnDestroy is called when component is removed        
        private void OnDestroy()
        {
            //Unset Delegates
            GestureMonitor.Instance.OnPinchLeft -= OnPinchLeft;
            GestureMonitor.Instance.OnPinchRight -= OnPinchRight;
            
            //Unset listeners
            _spatialAnchorCore.OnAnchorsLoadCompleted.RemoveListener(OnAnchorsLoadCompleted);
            _spatialAnchorCore.OnAnchorCreateCompleted.RemoveListener(OnAnchorCreateCompleted);

        }
        #endregion



        #region Public Methods
       
        /// <summary>
        /// This method switch Anchor Placement ON/OFF
        /// </summary>
        /// <param name="status">Is handling switch.</param>
        public static void HandleAnchorPlacement()
        {
            Instance._state = (Instance._state == StateAnchorManager.DISABLED) ? StateAnchorManager.ANCHORING : StateAnchorManager.DISABLED;
        }

        /// <summary>
        /// Creates an anchor from SafeArea transform 
        /// </summary>
        public static void CreateAnchor()
        {
            Instance._spatialAnchorCore.EraseAllAnchors();
            Instance._spatialAnchorSpawner.SpawnSpatialAnchor(Instance._transform.position, Instance._transform.rotation);
        }

        #endregion

        #region Private Methods


        /// <summary>
        /// This method is a callback for when SpacialAnchorCoreBuildingBlock finishes the Anchors Loading
        /// </summary>
        /// <param name="loadedAnchors">A list of OVRSpatialAnchors received by Core</param>
        private void OnAnchorsLoadCompleted(List<OVRSpatialAnchor> loadedAnchors)
        {
            Debug.Log($"[{nameof(AnchorManager)}] Anchors loaded successfully!");
            if (loadedAnchors.Count == 0)
            {
                Debug.LogWarning($"[{nameof(AnchorManager)}] None anchor loaded.");
                return;
            }
            OVRSpatialAnchor anchor = loadedAnchors[0];
            Vector3 position = anchor.transform.position;
            Quaternion rotation = Quaternion.Euler(0, anchor.transform.rotation.eulerAngles.y, 0);
            _transform.SetPositionAndRotation(position, rotation);
            _transform.gameObject.AddComponent<OVRSpatialAnchor>();
        }

        /// <summary>
        /// This method is a callback for when SpacialAnchorCoreBuildingBlock creates an anchor.
        /// </summary>
        /// <param name="anchor">Anchor created by Core.</param>
        /// <param name="result">Possible results of various AnchorCore operations.</param>
        private void OnAnchorCreateCompleted(OVRSpatialAnchor anchor, OVRSpatialAnchor.OperationResult result)
        {

            OVRSpatialAnchor currentAnchor;
            TryGetComponent<OVRSpatialAnchor>(out currentAnchor);

            if (currentAnchor == null)
                _transform.gameObject.AddComponent<OVRSpatialAnchor>();


            _state =(result.IsSuccess())?  StateAnchorManager.ANCHORED : StateAnchorManager.ANCHORING;

            if (!result.IsSuccess())
                Debug.LogError(result);

        }



        /// <summary>
        /// This method align the Anchor and SafeArea acording the spectate view
        /// </summary>
        private void Handling()
        {
            Vector3 forwardDirection = Camera.main.transform.forward;
            forwardDirection.y = 0;
            forwardDirection.Normalize();

            float cameraPitch = Camera.main.transform.eulerAngles.x;

            if (cameraPitch > 180)
            {
                cameraPitch -= 360;  // Limits the X axis rotation between -180 and 180 degrees.
            }

            float targetDistance = Mathf.Lerp(_maxDistance, 0f, Mathf.InverseLerp(-70f, 90f, cameraPitch));

            Vector3 targetPosition = Camera.main.transform.position + forwardDirection * targetDistance;
            Quaternion targetRotation = Quaternion.LookRotation(forwardDirection);

            //Sets Visual Anchor and Safe Area placement by the anchor
            _transform.SetPositionAndRotation(new Vector3(targetPosition.x, 0, targetPosition.z), targetRotation);
        }


        /// <summary>
        /// This Method is a callback from GestureMonitor Hand Left Pinch Action
        /// </summary>
        private void OnPinchLeft()
        {
            if (_state != StateAnchorManager.ANCHORED) return;
            _state = StateAnchorManager.ANCHORING;
        }


        /// <summary>
        /// This Method is a callback from GestureMonitor Hand Right Pinch Action
        /// </summary>
        private void OnPinchRight()
        {
            if(_state != StateAnchorManager.ANCHORING) return;
            CreateAnchor();

        }








        #endregion
    }

}