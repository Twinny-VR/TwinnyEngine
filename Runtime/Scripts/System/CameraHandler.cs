using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TMPro;
using Twinny.Helpers;
using Twinny.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Twinny.System.Cameras
{

    public class CameraHandler : TSingleton<CameraHandler>
    {
        #region Delegates
        public delegate void onCameraChanged(CinemachineFreeLook camera);
        public static onCameraChanged OnCameraChanged;
        public delegate void onCameraLocked(BuildingFeature building);
        public static onCameraLocked OnCameraLocked;
        #endregion

        [SerializeField] private CameraRuntime _config;

        private CinemachineBrain _brain;


        [Header("CAMERAS")]
        private CinemachineFreeLook _currentCamera;
        private CinemachineFreeLook currentCamera { get => _currentCamera; set { _currentCamera = value; OnCameraChanged?.Invoke(value); } }
        [SerializeField] private CinemachineFreeLook _centralCamera;
        [SerializeField] private CinemachineFreeLook _lockedCamera;
        private BuildingFeature _lockedCameraTarget;

        [Space]
        private CinemachineCameraOffset _offset;


        private float _zoomLimitMin = 0f;
        private float _zoomLimitMax = 1000f;

        [SerializeField] private Transform _sensorCenter;


        public float _xAxis = 0f;  // O valor que vai ser alterado com o arrasto
        [Range(0f, 1f)]
        public float _yAxis = .5f;  // O valor que vai ser alterado com o arrasto
        private float _initialX = 0f;  // O valor inicial antes de começar o arrasto
        private float _initialY = .5f;  // O valor inicial antes de começar o arrasto
        private float _touchStartX = 0f;  // Posição X do toque inicial
        private float _touchStartY = 0f;  // Posição Y do toque inicial
        private bool _isDragging = false;  // Para verificar se o usuário está arrastando
        private float _zoom;
        private float _initialDistance = 0f;

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

            OnCameraChanged += OnCameraChange;
            OnCameraLocked += OnLockInBuilding;

            GetComponent<CinemachineFreeLook>();

            currentCamera = _centralCamera;

            SetZoom(_zoom);
        }

        private void OnDestroy()
        {
            OnCameraChanged -= OnCameraChange;
            OnCameraLocked -= OnLockInBuilding;

        }



        // Update is called once per frame

        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            // Controle de zoom com sensibilidade ajustada
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                float zoomDelta = scrollInput;
                _zoom += zoomDelta * _config.zoomSensitivity * 1000f;  // Sensibilidade ajustada para o zoom
                _zoom = Mathf.Clamp(_zoom, _zoomLimitMin, _zoomLimitMax);   // Limita o zoom
                SetZoom(_zoom);
            }

            // Para dispositivos móveis e mouse, usando o toque ou clique
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

                            ray = Camera.main.ScreenPointToRay(touch.position);


                            if (Physics.Raycast(ray, out hit))
                            {

                                BuildingFeature building = hit.collider.gameObject.GetComponent<BuildingFeature>();

                                if (building && !_lockedCameraTarget)
                                {
                                    OnCameraLocked?.Invoke(building);
                                }
                            }

                            _touchStartX = touch.position.x;
                            _touchStartY = touch.position.y;
                            _initialX = currentCamera.m_XAxis.Value;
                            _initialY = currentCamera.m_YAxis.Value;
                            _isDragging = true;
                            break;

                        case TouchPhase.Moved:
                            if (_isDragging)
                            {
                                // Movimentos no eixo X (horizontal)
                                float deltaX = touch.position.x - _touchStartX;
                                if (Mathf.Abs(deltaX) > .5f) // Evita movimentos pequenos
                                {
                                    _xAxis = _initialX + deltaX * _config.horizontalSensitivity;
                                    SetHorizontalAxis(_xAxis);
                                }

                                // Movimentos no eixo Y (vertical)
                                float deltaY = touch.position.y - _touchStartY;
                                if (Mathf.Abs(deltaY) > .5f) // Evita movimentos pequenos
                                {
                                    _yAxis = _initialY + deltaY * _config.verticalSensitivity * .01f;
                                    _yAxis = Mathf.Clamp(_yAxis, 0f, 1f); // Limita o eixo Y
                                    SetGimbalVerticalAxis(_yAxis);
                                }
                            }
                            break;

                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            _isDragging = false;
                            break;
                    }

                    ray = Camera.main.ScreenPointToRay(touch.position);
                }
                else if (Input.touchCount == 2) // Zoom com dois toques
                {
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);

                    switch (touch1.phase)
                    {
                        case TouchPhase.Began:
                            _initialDistance = Vector2.Distance(touch1.position, touch2.position);
                            break;

                        case TouchPhase.Moved:
                            float currentDistance = Vector2.Distance(touch1.position, touch2.position);

                            //Debug.LogWarning($"DIST:{currentDistance}");

                            if (currentDistance > 100f && Mathf.Abs(currentDistance - _initialDistance) > 5f) // Limita o movimento para evitar zoom excessivo
                            {
                                float zoomDelta = (currentDistance - _initialDistance);
                                zoomDelta = Mathf.Clamp(zoomDelta, -1f, 1f); // Limita o zoom
                                _zoom += zoomDelta * _config.zoomSensitivity * 100f;
                                _zoom = Mathf.Clamp(_zoom, _zoomLimitMin, _zoomLimitMax);
                                SetZoom(_zoom);

                                _initialDistance = currentDistance;  // Atualiza a distância para o próximo movimento
                            }
                            break;
                    }
                }
                else // Para o mouse, quando não há toque
                {
                    Vector2 mousePosition = Input.mousePosition;
                    ray = Camera.main.ScreenPointToRay(mousePosition);




                    if (!_isDragging)
                    {



                        if (Physics.Raycast(ray, out hit))
                        {

                            BuildingFeature building = hit.collider.gameObject.GetComponent<BuildingFeature>();

                            if (building && _lockedCameraTarget == null)
                            {
                                OnCameraLocked?.Invoke(building);
                            }
                        }



                        _touchStartX = mousePosition.x;
                        _touchStartY = mousePosition.y;
                        _initialX = currentCamera.m_XAxis.Value;
                        _initialY = currentCamera.m_YAxis.Value;
                        _isDragging = true;
                    }
                    else
                    {
                        // Movimentos no eixo X (horizontal)
                        float deltaX = mousePosition.x - _touchStartX;
                        if (Mathf.Abs(deltaX) > 1f) // Evita movimentos pequenos
                        {
                            _xAxis = _initialX + deltaX * _config.horizontalSensitivity * .1f;// * 10f * Time.fixedDeltaTime;
                            SetHorizontalAxis(_xAxis);
                        }

                        // Movimentos no eixo Y (vertical)
                        float deltaY = mousePosition.y - _touchStartY;
                        if (Mathf.Abs(deltaY) > 1f) // Evita movimentos pequenos
                        {
                            _yAxis = _initialY + deltaY * _config.verticalSensitivity * .001f;// * .1f * Time.fixedDeltaTime;
                            _yAxis = Mathf.Clamp(_yAxis, 0f, 1f); // Limita o eixo Y
                            SetGimbalVerticalAxis(_yAxis);
                        }
                    }
                }



                // Verifica se um objeto foi clicado
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    GameObject clickedObject = hit.collider.gameObject;
                    SelectObject(clickedObject);
                }
            }
            else
            {
                _isDragging = false;
            }
        }

        #endregion

        #region Private Methods

        private void OnCameraChange(CinemachineFreeLook camera)
        {
            _offset = camera.GetComponent<CinemachineCameraOffset>();
            _lockedCamera.gameObject.SetActive(false);
            _centralCamera.gameObject.SetActive(false);


            if (camera == _centralCamera)
            {
                if (SceneFeature.Instance && SceneFeature.Instance.sensorCenter) LockCentralCamera(SceneFeature.Instance.sensorCenter);
                else
                    LockCentralCamera(_sensorCenter);
                _zoomLimitMin = _config.zoomLimitMin;
                _zoomLimitMax = _config.zoomLimitMax;
                CallBackUI.CallAction(callback => callback.OnCameraChanged(camera.transform, "CENTRAL"));
            }

            if (camera == _lockedCamera)
            {
                
                _zoomLimitMin = _lockedCameraTarget.GetLimiters().x;
                _zoomLimitMax = _lockedCameraTarget.GetLimiters().y;
                CallBackUI.CallAction(callback => callback.OnCameraChanged(camera.transform, "LOCKED"));
            }


            camera.gameObject.SetActive(true);


        }


        private void OnLockInBuilding(BuildingFeature building)
        {

            if (building == null)
            {
                //  _lockedCameraTarget.Select(false);
                _lockedCameraTarget = null;
                currentCamera = _centralCamera;
                CallBackUI.CallAction(callback => callback.OnCameraLocked(null));
                return;
            }
            if (building.overrideBlend)
            {
               
                var blend = _brain.m_CustomBlends.m_CustomBlends.FirstOrDefault(blend => blend.m_To == _lockedCamera.name);
                int index = 0; // _brain.m_CustomBlends.m_CustomBlends. (blend);
                CinemachineBlendDefinition oldBlend = blend.m_Blend;


                _brain.m_CustomBlends.m_CustomBlends[index].m_Blend = building.customBlend;


                AsyncOperationExtensions.CallDelayedAction(() => {

                    _brain.m_CustomBlends.m_CustomBlends[index].m_Blend = oldBlend;

                }, (int)(building.customBlend.m_Time*1000) + 100);
            }



            _lockedCameraTarget = building;
            //      _lockedCameraTarget.Select();
            _lockedCamera.Follow = _lockedCamera.LookAt = building.sensorCentral;
            currentCamera = _lockedCamera;

            CallBackUI.CallAction(callback => callback.OnCameraLocked(building.transform));


        }

        private void LockCentralCamera(Transform target)
        {
            Debug.Log($"[CameraHandler] Central Camera locked in: {target}");
            _centralCamera.Follow = target;
            _centralCamera.LookAt = target;
        }


        #endregion


        #region Public Methods
        public void SelectObject(GameObject go)
        {
            /*
            InteractableObject interactable = go.GetComponent<InteractableObject>();

            if (interactable != null)
            {

                interactable.HighLightObject(true);
            }
            */
        }




        public void SetHorizontalAxis(float value)
        {

            currentCamera.m_XAxis.Value = value;

        }


        public void SetVerticalAxis(float value)
        {
            Vector3 position = currentCamera.Follow.position;
            if (transform.position.y < 1f)
                position.y = 1f;
            else
                position.y = value;
            currentCamera.Follow.position = position;
            currentCamera.LookAt.position = position;
        }

        public void SetGimbalVerticalAxis(float value)
        {
            currentCamera.m_YAxis.Value = value;
        }


        public void SetZoom(float value)
        {
            Vector3 position = _offset.m_Offset;
            //float zoom = GetRealZoom(_yAxis);
            position.z = value;
            _offset.m_Offset = position;
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