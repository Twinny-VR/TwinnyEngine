using System.Collections;
using Unity.Cinemachine;
using Twinny.Helpers;
using Twinny.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;
using Concept.Helpers;
using Concept.Core;


namespace Twinny.System.Cameras
{

    public enum State
    {
        FPS = 0,
        LOCKEDTHIRD = 1,
        LOCKED = 2,
        PAN = 3,
    }

    public class OldCameraManager : TSingleton<OldCameraManager>
    {
        [SerializeField]
        private State _state;
        public static State state
        {
            get => Instance._state; set
            {
                Instance._state = value;
                OnStateChanged.Invoke(value);
            }
        }


        [SerializeField]
        private OldInterestItem _interestItem;
        public static OldInterestItem interestItem { get { return Instance._interestItem; } }

        #region Delegates
        public delegate void onStateChanged(State state);
        public static onStateChanged OnStateChanged;

        public delegate void onCameraChanged(CameraHandler2 camera);
        public static onCameraChanged OnCameraChanged;

        public delegate void onCameraLocked(BuildingFeature building);
        public static onCameraLocked OnCameraLocked;

        public delegate void onEnterInStandby();
        public static onEnterInStandby OnEnterInStandby;


        #endregion


        public static CinemachineBrain brain { get => Instance?._brain; }


        [Header("CAMERAS")]
        private CameraHandler2 _currentCamera;
        //private CinemachineVirtualCamera currentCamera { get => _currentCamera; set { _currentCamera = value; OnCameraChanged?.Invoke(value); } }
        [SerializeField] private CameraHandler2 _fpsCamera;
        public static CameraHandler2 fpsCamera { get => Instance._fpsCamera; }
        [SerializeField] private CameraHandler2 _lockedCamera;
        [SerializeField] private CameraHandler2 _panoramicCamera;
        [SerializeField] private CameraHandler2 _thirdCamera;

        [DrawScriptable]
        [SerializeField] private CameraRuntime _config;
        public static CameraRuntime config { get => Instance?._config; }

        private OldInterestItem _defaultCentralSensor;

        private Coroutine _standbyCor;
        private CinemachineBrain _brain;
        /*
        private bool _isInStandby = false;
        public bool isInStandBy { get => _isInStandby; }

        [SerializeField] private Transform _sensorFollow;
        [SerializeField] private Transform _sensorLook;
        */

        public static float zoom { get => Instance._currentCamera.zoom; }

        #region MonoBehaviour Methods

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
            string resPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.Refresh();
            }

            string fileName = "CameraRuntimePreset.asset";
            string assetPath = resPath + "/" + fileName;


            CameraRuntime preset = AssetDatabase.LoadAssetAtPath<CameraRuntime>(assetPath);

            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<CameraRuntime>();
                AssetDatabase.CreateAsset(preset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.LogWarning("[CameraHandler] Novo preset 'CameraRuntimePreset' criado e salvo em: " + assetPath);
            }

            _config = AssetDatabase.LoadAssetAtPath<CameraRuntime>(assetPath);

        }

#endif


        protected override void Awake()
        {
            base.Awake();
            _brain = Camera.main.GetComponent<CinemachineBrain>();

            _config = Resources.Load<CameraRuntime>("CameraRuntimePreset");

            if (_config == null)
            {
                Debug.LogError("[CameraHandler] Impossible to load 'CameraRuntimePreset'.");
            }

        }

        protected override void Start()
        {
            base.Start();

            OnStateChanged += OnStateChange;
            OnCameraLocked += OnLockInBuilding;
            LevelManager.OnExperienceFinished += OnExperienceFinished;
            InputMonitor.OnSelect += OnObjectSelected;
            InputMonitor.OnTouch += OnTouch;
            InputMonitor.OnRelease += OnRelease;
            InputMonitor.OnCancelDrag += OnCancelDrag;
           // _defaultCentralSensor = _panoramicCamera.follow.GetComponent<InterestItem>(); 
        }

        private void OnDestroy()
        {
            OnStateChanged -= OnStateChange;
            OnCameraLocked -= OnLockInBuilding;
            LevelManager.OnExperienceFinished -= OnExperienceFinished;
            InputMonitor.OnSelect -= OnObjectSelected;
            InputMonitor.OnTouch -= OnTouch;
            InputMonitor.OnRelease -= OnRelease;
            InputMonitor.OnCancelDrag -= OnCancelDrag;

        }

        #endregion

        #region Private Methods

        private void OnExperienceFinished()
        {
            StopAllCoroutines();
        }
        private void OnStateChange(State state)
        {

            CameraHandler2 camera = null;

            switch (state)
            {
                case State.FPS:
                    camera = Instance._fpsCamera;
                    Debug.Log("[CameraManager] First Person Camera.");
                    break;
                case State.LOCKED:
                    camera = Instance._lockedCamera;
                    Debug.Log("[CameraManager] CloseUp Camera.");
                    break;
                case State.PAN:
                    camera = Instance._panoramicCamera;
                    Debug.Log("[CameraManager] Panoramic Camera.");
                    break;
                case State.LOCKEDTHIRD:
                    camera = Instance._thirdCamera;
                    Debug.Log("[CameraManager] Third Person Camera.");
                    break;
                default:
                    Debug.LogError("[CameraManager] Unknow Camera.");
                    break;
            }

            OnCameraChange(camera);
        }

        private void OnCameraChange(CameraHandler2 camera)
        {

            if (_currentCamera)
            {
                _currentCamera.priority = 10;
                _currentCamera.enabled = false;
            }
            _currentCamera = camera;
            OnCameraChanged?.Invoke(camera);
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnCameraChanged(camera.transform, state.ToString()));
            /*
            if (camera == _panoramicCamera)
            {
                if (SceneFeature.Instance && SceneFeature.Instance.sensorFollow && SceneFeature.Instance.sensorLook) LockCentralCamera(SceneFeature.Instance.sensorFollow, SceneFeature.Instance.sensorLook);
                else
                    LockCentralCamera(_sensorFollow, _sensorLook);
                CallBackUI.CallAction<IUICallBacks>(callback => callback.OnCameraChanged(camera.transform, "CENTRAL"));
            }
            else
            if (camera == _lockedCamera)
            {

                //_zoomLimitMin = _lockedCameraTarget.GetLimiters().x;
                //_zoomLimitMax = _lockedCameraTarget.GetLimiters().y;
                CallBackUI.CallAction<IUICallBacks>(callback => callback.OnCameraChanged(camera.transform, "LOCKED"));
            }
            */


            Debug.LogWarning("RESETA PRA: " + _interestItem.name);
            camera.ResetCamera();
            StartStandby();

        }


        //TODO Remover
        private void OnLockInBuilding(BuildingFeature building)
        {

            if (building == null)
            {
                //  _lockedCameraTarget.Select(false);
                state = State.PAN;
                var feature = SceneFeature.Instance;
                 
                SwitchCamera(feature ? feature.interestPoints[0].GetComponent<OldInterestItem>() : _defaultCentralSensor);
                CallbackHub.CallAction<IUICallBacks>(callback => callback.OnCameraLocked(null));

                return;
            }

            /*
            if (building.overrideBlend)
            {

                var blend = _brain.m_CustomBlends.m_CustomBlends.FirstOrDefault(blend => blend.m_To == _lockedCamera.name);
                int index = 0; // _brain.m_CustomBlends.m_CustomBlends. (blend);
                CinemachineBlendDefinition oldBlend = blend.m_Blend;


                _brain.m_CustomBlends.m_CustomBlends[index].m_Blend = building.customBlend;


                AsyncOperationExtensions.CallDelayedAction(() =>
                {

                    _brain.m_CustomBlends.m_CustomBlends[index].m_Blend = oldBlend;

                }, (int)(building.customBlend.m_Time * 1000) + 100);
            }
            */

            if (building.type == State.LOCKED || building.type == State.LOCKEDTHIRD)
            {

                SwitchCamera(building);
                CallbackHub.CallAction<IUICallBacks>(callback => callback.OnCameraLocked(building.centralSensor));

            }
            else
                Debug.LogError("[CameraManager] Locked camera error! Only 'BuildingFeature' type 'Locked' or 'LockedThird' are supported.");



        }


        private void StartStandby()
        {
            if (!config.allowStandby) return;

            if (_standbyCor != null)
            {
                StopCoroutine(_standbyCor);
                CallbackHub.CallAction<IUICallBacks>(callback => callback.OnStandby(false));

            }
            _standbyCor = StartCoroutine(StandByCor());
        }

        #endregion


        #region Public Methods

        public static void SwitchCamera(OldInterestItem interest)
        {

            if (interest && Instance)
            {
                Instance._interestItem = interest;

                if (interest.type != State.PAN)
                {
                    Transform nodePosition = (interest is BuildingFeature) ? (interest as BuildingFeature).facadeTeleportNode : interest.transform;
                    FirstPersonAgent.TeleportTo(nodePosition);

                }
//                if (interest.type == State.FPS || interest.type == State.LOCKEDTHIRD) SetAgentPosition(interest.transform);

            }


            state = interest == null ? State.FPS : interest.type;

        }


        public void OnTouch(float x, float y)
        {
            StartStandby();
        }

        public void OnRelease(float x, float y)
        {
            StartStandby();
        }

        public void OnCancelDrag(float x, float y)
        {
            StartStandby();
        }

        public void OnObjectSelected(RaycastHit hit)
        {

            if (brain.IsBlending) return;


            BuildingFeature building = hit.collider.gameObject.GetComponent<BuildingFeature>();
            if (_state == State.PAN &&  building)
            {
                OnCameraLocked?.Invoke(building);
                return;
            }
        }



        #endregion


        #region Coroutines


        private IEnumerator StandByCor()
        {
            bool restart = false;
            yield return new WaitForSeconds(config.standbyInactiveTime);

            if (InputMonitor.isDragging) yield break;

            Debug.LogWarning("[CameraManager] STAND BY MODE.");
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnStandby(true));

            OnEnterInStandby?.Invoke();
            var feature = SceneFeature.Instance;
            if (state == State.PAN && feature && feature.interestPoints.Length > 0)
            {
                yield return new WaitForSeconds(config.standbyPanoramicDuration);

                foreach (var interest in feature.interestPoints)
                {
                    BuildingFeature building = interest.GetComponent<BuildingFeature>();

                    if (building)
                    {

                        _ = CanvasTransition.FadeScreen(true);
                        yield return new WaitForSeconds(TwinnyManager.config.fadeTime);
                        OnCameraLocked?.Invoke(building);
                        yield return new WaitForSeconds(1f);
                        while (brain.IsBlending)
                        {
                            yield return null;
                        }
                        _ = CanvasTransition.FadeScreen(false);
                        yield return new WaitForSeconds(3f);
                        OnEnterInStandby?.Invoke();
                        yield return new WaitForSeconds(config.standbyLockedDuration);
                    }
                }
                restart = true;
            }
            else
                if (state == State.LOCKED)
            {
                yield return new WaitForSeconds(config.standbyLockedDuration);
                restart = true;
            }

            if (restart)
            {
                OnCameraLocked?.Invoke(null);
                StartStandby();//Restart StandBy 
            }

        }


        #endregion


        float GetRealZoom(float value)
        {
            if (value <= 0)
            {
                return 1; // Caso A seja 0, B é 2
            }
            else if (value >= 1)
            {
                return 1; // Caso A seja 1, B também é 2
            }
            else if (value <= 0.5f)
            {
                // Interpolação entre 2 e 10 para A entre 0 e 0.5
                return 1 + (value * (10 - 1) / 0.5f); // Aumenta de 2 até 10 quando A vai de 0 a 0.5
            }
            else
            {
                // Interpolação entre 10 e 2 para A entre 0.5 e 1
                return 10 - ((value - 0.5f) * (10 - 1) / 0.5f); // Diminui de 10 até 2 quando A vai de 0.5 a 1
            }
        }

    }

}