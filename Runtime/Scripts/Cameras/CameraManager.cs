#if !OCULUS
using System.Collections;
using Cinemachine;
using Twinny.Helpers;
using Twinny.UI;
using UnityEditor;
using UnityEngine;
using static Twinny.System.LevelManager;


namespace Twinny.System.Cameras
{

    public enum CameraState
    {
        FPS,
        LOCKED,
        PAN,
        THIRD
    }


    public class CameraManager : TSingleton<CameraManager>
    {

        [SerializeField]
        private CameraState _state;
        public static CameraState state
        {
            get => Instance._state; set
            {
                Instance._state = value;
                OnStateChanged.Invoke(value);
            }
        }

        public FirstPersonAgent fpsAgent;

        #region Delegates
        public delegate void onStateChanged(CameraState state);
        public static onStateChanged OnStateChanged;

        public delegate void onCameraChanged(CameraHandler camera);
        public static onCameraChanged OnCameraChanged;

        public delegate void onCameraLocked(BuildingFeature building);
        public static onCameraLocked OnCameraLocked;

        public delegate void onEnterInStandby();
        public static onEnterInStandby OnEnterInStandby;


        #endregion

        [SerializeField] private CameraRuntime _config;
        public static CameraRuntime config { get => Instance._config; }

        private CinemachineBrain _brain;
        public static CinemachineBrain brain { get => Instance._brain; }


        [Header("CAMERAS")]
        private CameraHandler _currentCamera;
        //private CinemachineVirtualCamera currentCamera { get => _currentCamera; set { _currentCamera = value; OnCameraChanged?.Invoke(value); } }
        [SerializeField] private CameraHandler _fpsCamera;
        [SerializeField] private CameraHandler _lockedCamera;
        [SerializeField] private CameraHandler _panoramicCamera;
        [SerializeField] private CameraHandler _thirdCamera;

        private Coroutine _standbyCor;
        /*
        private bool _isInStandby = false;
        public bool isInStandBy { get => _isInStandby; }

        [SerializeField] private Transform _sensorFollow;
        [SerializeField] private Transform _sensorLook;
        */

        public static float zoom { get => Instance._currentCamera.zoom; }
        
        #region MonoBehaviour Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (AssetDatabase.IsValidFolder("Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string fileName = "CameraRuntimePreset.asset";
            string assetPath = "Assets/Resources/" + fileName;
            CameraRuntime preset = AssetDatabase.LoadAssetAtPath<CameraRuntime>(assetPath);

            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<CameraRuntime>();
                AssetDatabase.CreateAsset(preset, assetPath);
                AssetDatabase.SaveAssets();
                Debug.LogWarning("[CameraHandler] Novo preset 'CameraRuntimePreset' criado e salvo em: " + assetPath);
            }

            _config = AssetDatabase.LoadAssetAtPath<CameraRuntime>(assetPath);

        }

        [ContextMenu("FPS Camera")]
        private void SetFpsCamera()
        {
            state = CameraState.FPS;
        }

        public void SetFpsCamera(Vector3 position)
        {
            
            state = CameraState.FPS;

        }

        [ContextMenu("Third Camera")]
        private void SetThirdCamera()
        {
            state = CameraState.THIRD;
        }

        [ContextMenu("Panoramic Camera")]
        private void SetPanCamera()
        {
            state = CameraState.PAN;
        }



#endif


        void Awake()
        {
            _brain = Camera.main.GetComponent<CinemachineBrain>();

            _config = Resources.Load<CameraRuntime>("CameraRuntimePreset");

            if (_config == null)
            {
                Debug.LogError("[CameraHandler] Impossible to load 'CameraRuntimePreset'.");
            }

        }


        // Start is called before the first frame update
        void Start()
        {
            Init();

            OnStateChanged += OnStateChange;
            OnCameraLocked += OnLockInBuilding;
            LevelManager.OnExperienceFinished += OnExperienceFinished;
            InputMonitor.OnSelect += OnObjectSelected;
            InputMonitor.OnTouch += OnTouch;
            InputMonitor.OnRelease += OnRelease;
            InputMonitor.OnCancelDrag += OnCancelDrag;
            GetComponent<CinemachineFreeLook>();

            state = _state;
            StartStandby();
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

        /*
        private void Update()
        {
            if (Input.GetMouseButton(0) || Input.touchCount > 0)
            {
                Ray ray = default;
                RaycastHit hit;



                if (Input.touchCount == 1) // Para um toque, movimento horizontal e vertical
                {
                    Touch touch = Input.GetTouch(0);  // Captura o primeiro toque
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            _isDragging = true;

                            ray = Camera.main.ScreenPointToRay(touch.position);


                            if (Physics.Raycast(ray, out hit))
                            {

                                BuildingFeature building = hit.collider.gameObject.GetComponent<BuildingFeature>();

                                if (building && !_lockedCameraTarget)
                                {
                                    OnCameraLocked?.Invoke(building);
                                }
                            }
                            break;
                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            _isDragging = false;
                            break;
                    }

                    // Verifica se um objeto foi clicado
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        GameObject clickedObject = hit.collider.gameObject;
                        SelectObject(clickedObject);
                    }
                }
                else if (Input.touchCount == 2) // Zoom com dois toques
                {
                }
                else // Para o mouse, quando não há toque
                {
                    Vector2 mousePosition = Input.mousePosition;
                    ray = Camera.main.ScreenPointToRay(mousePosition);




                    if (!_isDragging)
                    {

                        _isDragging = true;


                        if (Physics.Raycast(ray, out hit))
                        {

                            BuildingFeature building = hit.collider.gameObject.GetComponent<BuildingFeature>();

                            if (building && _lockedCameraTarget == null)
                            {
                                OnCameraLocked?.Invoke(building);
                            }
                        }
                    }
                }
            } else
                _isDragging = false;

        }
        */
        #endregion

        #region Private Methods

        private void OnExperienceFinished()
        {
            StopAllCoroutines();
        }
        private void OnStateChange(CameraState state)
        {

            CameraHandler camera = null;

            switch (state)
            {
                case CameraState.FPS:
                    camera = Instance._fpsCamera;
                    Debug.Log("[CameraManager] First Person Camera.");
                    break;
                case CameraState.LOCKED:
                    camera = Instance._lockedCamera;
                    Debug.Log("[CameraManager] CloseUp Camera.");
                    break;
                case CameraState.PAN:
                    camera = Instance._panoramicCamera;
                    Debug.Log("[CameraManager] Panoramic Camera.");
                    break;
                case CameraState.THIRD:
                    camera = Instance._thirdCamera;
                    Debug.Log("[CameraManager] Third Person Camera.");
                    break;
                default:
                    Debug.LogError("[CameraManager] Unknow Camera.");
                    break;
            }

            OnCameraChange(camera);
        }

        private void OnCameraChange(CameraHandler camera)
        {

            if (_currentCamera)
            {
                _currentCamera.priority = 10;
                _currentCamera.enabled = false;
            }
            _currentCamera = camera;
            OnCameraChanged?.Invoke(camera);
            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnCameraChanged(camera.transform, state.ToString()));
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

            camera.ResetCamera();

        }


        //TODO Remover
        private void OnLockInBuilding(BuildingFeature building)
        {

            if (building == null)
            {
                //  _lockedCameraTarget.Select(false);
                state = CameraState.PAN;
                CallBackUI.CallAction<IUICallBacks>(callback => callback.OnCameraLocked(null));
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

            _lockedCamera.follow = building.sensorCentral.transform;
            _lockedCamera.lookAt = building.sensorCentral.transform;
            _lockedCamera.fov = building.sensorCentral.desiredFov;
            state = CameraState.LOCKED;

            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnCameraLocked(building.transform));


        }

        private void LockCentralCamera(Transform follow, Transform look)
        {
            Debug.Log($"[CameraHandler] Central Camera locked in: {follow} LookAt: {look}");
            _panoramicCamera.follow = follow;
            _panoramicCamera.lookAt = look;
        }

        private void StartStandby()
        {
            if (!config.allowStandby) return;

            if (_standbyCor != null)
            {
                StopCoroutine(_standbyCor);
                CallBackUI.CallAction<IUICallBacks>(callback => callback.OnStandby(false));

            }
            _standbyCor = StartCoroutine(StandByCor());
        }

        #endregion


        #region Public Methods

        public static void SwitchCameraState(CameraState newState)
        {
            state = newState;

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
            if (building)
            {
                OnCameraLocked?.Invoke(building);
                return;
            }
        }


        public static void SetFPS(Transform node)
        {
            OnCameraLocked?.Invoke(null);

            if (node)
            {
                Instance.fpsAgent.transform.position = node.position;
                Instance.fpsAgent.transform.rotation = node.rotation;
            }

            SwitchCameraState(CameraState.FPS);

        }


        #endregion


        #region Coroutines


        private IEnumerator StandByCor()
        {
            bool restart = false;
            yield return new WaitForSeconds(config.standbyInactiveTime);

            if (InputMonitor.isDragging) yield break;

            Debug.LogWarning("[CameraManager] STAND BY MODE.");
            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnStandby(true));

            OnEnterInStandby?.Invoke();
            if (state == CameraState.PAN && SceneFeature.Instance && SceneFeature.Instance.centralBuildings.Length > 0)
            {
                yield return new WaitForSeconds(config.standbyPanoramicDuration);

                foreach (var building in SceneFeature.Instance.centralBuildings)
                {
                    _ = CanvasTransition.FadeScreen(true);
                    yield return new WaitForSeconds(Config.fadeTime);
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
                restart = true;
            }else
                if(state == CameraState.LOCKED)
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

#endif