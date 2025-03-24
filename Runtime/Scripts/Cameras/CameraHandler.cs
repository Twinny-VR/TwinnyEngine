#if !OCULUS
using System.Collections;
using Cinemachine;
using UnityEngine;
using static Twinny.System.Cameras.CameraManager;
using Twinny.UI;
using Twinny.Helpers;

namespace Twinny.System.Cameras
{
    public class CameraHandler : MonoBehaviour, IInputCallBacks
    {
        #region Properties

        [SerializeField] State cameraType;
        private CinemachineVirtualCameraBase _virtualCamera;

        private CinemachineCameraOffset _offset;
        private CinemachinePOV _pov;
        private CinemachineOrbitalTransposer _transposer;

        private float _initialX = 0f;  // O valor inicial antes de começar o arrasto
        private float _initialY = .5f;  // O valor inicial antes de começar o arrasto


        public float _xAxis = 0f;  // O valor que vai ser alterado com o arrasto
        public float _yAxis = .5f;  // O valor que vai ser alterado com o arrasto


        private float _initialYaw = 0f;
        private float _initialZoom = 0;
        private float _zoom = 0;
        public float zoom { get => _zoom; set => _zoom = value; }

        public float fov { get => (_virtualCamera as CinemachineVirtualCamera).m_Lens.FieldOfView; set { (_virtualCamera as CinemachineVirtualCamera).m_Lens.FieldOfView = value; } }
        public Transform follow { get => _virtualCamera.Follow; set { _virtualCamera.Follow = value; } }
        public Transform lookAt { get => _virtualCamera.LookAt; set { _virtualCamera.LookAt = value; } }
        public int priority { get => _virtualCamera.Priority; set { _virtualCamera.Priority = value; } }

        private Coroutine _recenterCor;
        private Coroutine _standbyCor;

        //Config variables

        private float _yawRangeMin;
        private float _yawRangeMax;
        private float _yawRadius;
        private float _yawSpeedMultiply;
        private float _zoomSpeedMultiply;
        private float _zoomMin;
        private float _zoomMax;



        #endregion

        #region MonoBehaviour Methods

        private void Awake()
        {
            _virtualCamera = GetComponent<CinemachineVirtualCameraBase>();
            _offset = GetComponent<CinemachineCameraOffset>();
            _pov = (_virtualCamera as CinemachineVirtualCamera).GetCinemachineComponent<CinemachinePOV>();
            _transposer = (_virtualCamera as CinemachineVirtualCamera).GetCinemachineComponent<CinemachineOrbitalTransposer>();
            enabled = false;
        }

        // Start is called before the first frame update
        void Start()
        {

            CallBackUI.RegisterCallback(this);

            LevelManager.OnExperienceFinished += OnExperienceFinished;
            OnEnterInStandby += SetStandby;

            switch (cameraType)
            {
                case State.FPS:
                    fov = config.fpsDesiredFov; break;
                case State.PAN:
                    fov = config.panDesiredFov; break;
                case State.LOCKEDTHIRD:
                    fov = config.thirdDesiredFov; break;
            }

        }

        private void OnDestroy()
        {
            CallBackUI.UnregisterCallback(this);
            LevelManager.OnExperienceFinished -= OnExperienceFinished;
            OnEnterInStandby -= SetStandby;

        }

        #endregion

        #region Public Methods
        public void ResetCamera()
        {
            if (interestItem)
            {

                Transform target = (interestItem is BuildingFeature) ? (interestItem as BuildingFeature).centralSensor : interestItem.transform;

                if (cameraType != State.FPS)
                {
                    follow = target;
                    lookAt = target;
                }


                if (interestItem.overrideCameraSettings)
                {
                    fov = interestItem.desiredFov;
                    _yawRangeMin = interestItem.yawRange.x;
                    _yawRangeMax = interestItem.yawRange.y;
                    _yawRadius = interestItem.yawRange.z;
                    _yawSpeedMultiply = interestItem.yawSpeedMultiply;
                    _zoomSpeedMultiply = interestItem.zoomSpeedMultiply;
                    _zoomMin = interestItem.zoomRange.x;
                    _zoomMax = interestItem.zoomRange.y;
                }
                else
                {
                    switch (cameraType)
                    {
                        case State.FPS:
                            fov = config.fpsDesiredFov;
                            _yawRangeMin = config.fpsYawRange.x;
                            _yawRangeMax = config.fpsYawRange.y;
                            _yawRadius = config.fpsYawRange.z;
                            _yawSpeedMultiply = config.fpsYawSpeedMultiply;
                            break;
                        case State.LOCKEDTHIRD:
                            fov = config.thirdDesiredFov;
                            _yawRangeMin = config.thirdYawRange.x;
                            _yawRangeMax = config.thirdYawRange.y;
                            _yawRadius = config.thirdYawRange.z;
                            _yawSpeedMultiply = config.thirdYawSpeedMultiply;
                            _zoomSpeedMultiply = config.thirdZoomSpeedMultiply;
                            _zoomMin = config.thirdZoomRange.x;
                            _zoomMax = config.thirdZoomRange.y;
                            break;
                        case State.LOCKED:
                            fov = config.lockedDesiredFov;
                            _yawRangeMin = config.lockedYawRange.x;
                            _yawRangeMax = config.lockedYawRange.y;
                            _yawRadius = config.lockedYawRange.z;
                            _yawSpeedMultiply = config.lockedYawSpeedMultiply;
                            _zoomSpeedMultiply = config.lockedZoomSpeedMultiply;
                            _zoomMin = config.lockedZoomRange.x;
                            _zoomMax = config.lockedZoomRange.y;
                            break;
                        case State.PAN:
                            fov = config.panDesiredFov;
                            _yawRangeMin = config.panYawRange.x;
                            _yawRangeMax = config.panYawRange.y;
                            _yawRadius = config.panYawRange.z;
                            _yawSpeedMultiply = config.panYawSpeedMultiply;
                            _zoomSpeedMultiply = config.panZoomSpeedMultiply;
                            _zoomMin = config.panZoomRange.x;
                            _zoomMax = config.panZoomRange.y;
                            break;
                        default:
                            break;
                    }
                }


                if (_pov)
                {
                    _pov.m_VerticalAxis.Value = 0;
                    _pov.m_HorizontalAxis.Value = target.eulerAngles.y;
                }


                _initialYaw = cameraType == State.FPS ? 0 : (_yawRangeMax - _yawRangeMin) / 2;
                if (_transposer && cameraType != State.FPS)
                {
                    _transposer.m_FollowOffset.y = _initialYaw;
                    _transposer.m_FollowOffset.z = _yawRadius;
                }


                _zoom = _initialZoom = _zoomMin;
                if (_offset) _offset.m_Offset.z = _zoom;

                if (interestItem.overrideCameraBlend)
                {
                    brain.m_DefaultBlend = interestItem.cameraBlend;
                }
                else
                {
                    switch (cameraType)
                    {
                        case State.FPS:
                            brain.m_DefaultBlend = config.fpsCameraBlend;
                            break;
                        case State.LOCKEDTHIRD:
                            brain.m_DefaultBlend = config.thirdCameraBlend;
                            break;
                        case State.LOCKED:
                            brain.m_DefaultBlend = config.lockedCameraBlend;
                            break;
                        case State.PAN:
                            brain.m_DefaultBlend = config.panCameraBlend;
                            break;
                        default:
                            break;
                    }
                }
            }



           priority = 11;
           enabled = true;
           
            AsyncOperationExtensions.CallDelayedAction(()=> { 
            },1000);

        }


        private void SetStandby()
        {
            if (!isActiveAndEnabled) return;

            if (InputMonitor.isDragging) return;

            if (_recenterCor != null) StopCoroutine(_recenterCor);

            _recenterCor = StartCoroutine(Recenter());

            if (_standbyCor != null) StopCoroutine(_standbyCor);

            _standbyCor = StartCoroutine(StandBy());
        }

        private void StopStandBy()
        {
            if (_recenterCor != null) StopCoroutine(_recenterCor);
            if (_standbyCor != null) StopCoroutine(_standbyCor);

        }

        #endregion

        #region Private Methods
        private void OnExperienceFinished()
        {
            StopAllCoroutines();
        }

        private void SetHorizontalAxis(float value)
        {
            if (!isActiveAndEnabled) return;


            switch (cameraType)
            {
                case State.FPS:
                    _xAxis = _initialX - value * config.sesitivity.x;
                    if (_pov)
                        _pov.m_HorizontalAxis.Value = _xAxis;
                    else
                        Debug.LogError($"[CameraHandler] POV is missing in {name} game object.");
                    break;
                case State.LOCKED:
                case State.LOCKEDTHIRD:
                case State.PAN:
            _xAxis = _initialX + value * config.sesitivity.x;
                    if (_transposer)
                        _transposer.m_XAxis.Value = _xAxis;
                    else
                        Debug.LogError($"[CameraHandler] Transposer is missing in {name} game object.");
                    break;
                default:
                    break;
            }



        }

        private void SetVerticalAxis(float value)
        {
            if (!isActiveAndEnabled) return;


            switch (cameraType)
            {
                case State.FPS:
                    _yAxis = _initialY + value * config.sesitivity.y;
                    if (_pov)
                        _pov.m_VerticalAxis.Value = _yAxis;
                    else
                        Debug.LogError($"[CameraHandler] POV is missing in {name} game object.");
                    break;
                case State.LOCKED:
                case State.LOCKEDTHIRD:
                case State.PAN:
                    _yAxis = _initialY + value * -config.sesitivity.y * _yawSpeedMultiply;
                    if (_transposer)
                    {

                        _yAxis = Mathf.Clamp(_yAxis, _yawRangeMin, _yawRangeMax);

                        _transposer.m_FollowOffset.y = _yAxis;

                    }
                    else
                        Debug.LogError($"[CameraHandler] Transposer is missing in {name} game object.");
                    break;
                default:
                    break;
            }


            /*
            Vector3 position = _virtualCamera.Follow.position;
            if (transform.position.y < 1f)
                position.y = 1f;
            else
                position.y = value;
            _virtualCamera.Follow.position = position;
            _virtualCamera.LookAt.position = position;
            */
        }

        private void SetZoom(float value)
        {
            if (!isActiveAndEnabled) return;


            switch (cameraType)
            {
                case State.FPS: //No zoom for FPS Camera
                    if (interestItem.type == State.LOCKEDTHIRD && value < 0)
                        SwitchCamera(interestItem);
                    return;
                case State.LOCKED:
                case State.LOCKEDTHIRD:
                case State.PAN:



                    if (interestItem)
                    {
                        _zoom += value * config.sesitivity.z * 100f * _zoomSpeedMultiply;
                        _zoom = Mathf.Clamp(_zoom, _zoomMin, _zoomMax);

                    }
                    else
                        Debug.LogError($"[CameraHandler] Subject Interest is missing in {name} game object.");

                    if (_offset)
                    {
                        if (interestItem.type == State.LOCKEDTHIRD && _zoom >= _zoomMax)
                            SwitchCamera(null);


                        _offset.m_Offset.z = _zoom;
                    }
                    else
                        Debug.LogError($"[CameraHandler] Offset is missing in {name} game object.");


                    break;
                /*
                                case State.THIRD:
                                    _zoom += value * config.zoomSensitivity * 100f;
                                    _zoom = Mathf.Clamp(_zoom, config.thirdZoomLimitMin, config.thirdZoomLimitMax);

                                    if (_offset)
                                    {
                                        if (_zoom >= config.thirdZoomLimitMax)
                                            SwitchCameraState(State.FPS);

                                        _offset.m_Offset.z = _zoom;
                                    }
                                    else
                                        Debug.LogError($"[CameraHandler] Offset is missing in {name} game object.");

                                    break;
                  */
                default:
                    Debug.LogError($"[CameraHandler] Unknow Camera in {name} game object.");
                    break;
            }


            /*
            Vector3 position = _offset.m_Offset;
            //float zoom = GetRealZoom(_yAxis);
            position.z = value;
            _offset.m_Offset = position;
            */
        }


        #endregion

        #region IInputCallBacks methods

        public void OnTouch(float x, float y)
        {
            if (!isActiveAndEnabled) return;


            StopStandBy();


            switch (cameraType)
            {
                case State.FPS:
                    if (_pov)
                    {
                        _initialX = _pov.m_HorizontalAxis.Value;
                        _initialY = _pov.m_VerticalAxis.Value;
                    }
                    else
                        Debug.LogError($"[CameraHandler] POV is missing in {name} game object.");
                    break;
                case State.LOCKED:
                case State.LOCKEDTHIRD:
                case State.PAN:
                    if (_transposer)
                    {
                        _initialX = _transposer.m_XAxis.Value;
                        _initialY = _transposer.m_FollowOffset.y;

                    }
                    else
                        Debug.LogError($"[CameraHandler] Transposer is missing in {name} game object.");
                    break;
                default:
                    break;
            }




        }

        public void OnRelease(float x, float y)
        {
            Debug.LogWarning($"RELEASE X:{x} Y:{y}.");
        }

        public void OnThreeFingersDragging(float x, float y)
        {
            if (brain.IsBlending) return;

            Debug.LogWarning($"PALM DRAGGING X:{x} Y:{y}.");


        }

        public void OnPinchingStart(float factor) { }
        public void OnPinching(float factor)
        {

            if (brain.IsBlending) return;


            SetZoom(factor);
        }

        public void OnDraggingHorizontal(float factor)
        {
            if (brain.IsBlending) return;

            SetHorizontalAxis(factor);
        }

        public void OnDraggingVertical(float factor)
        {
            if (brain.IsBlending) return;

            SetVerticalAxis(factor);

        }

        public void OnDragEnded(float x, float y)
        {
            Debug.LogWarning($"DRAG ENDED at X:{x} Y:{y}.");
        }

        public void OnSelect(object sender)
        {




            //if (building && _lockedCameraTarget == null)
            //  OnCameraLocked?.Invoke(building);



        }

        #endregion

        #region Coroutines


        public IEnumerator Recenter()
        {
            float elapsed = 0f;
            float duration = config.standbyRecenterTime;
            float startY = 0f;

            if (_pov)
                startY = _pov.m_VerticalAxis.Value;
            else
                if (_transposer)
                startY = _transposer.m_FollowOffset.y;

            float startZ = _offset ? _offset.m_Offset.z : 0;

            while (elapsed < duration)
            {

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (_pov)
                {
                    _pov.m_VerticalAxis.Value = Mathf.Lerp(startY, _initialYaw, t);
                }
                else
                if (_transposer)
                {
                    _transposer.m_FollowOffset.y = Mathf.Lerp(startY, _initialYaw, t);
                }


                if (_offset)
                    _offset.m_Offset.z = Mathf.Lerp(startZ, _initialZoom, t);


                yield return null;
            }
        }

        public IEnumerator StandBy()
        {

            float elapsed = 0f;

            while (true)
            {

                elapsed += Time.deltaTime;

                if (_pov)
                {

                    _pov.m_HorizontalAxis.Value += config.standbyRotationSpeed * Time.deltaTime;
                }
                else
                if (_transposer)
                {
                    _transposer.m_XAxis.Value += config.standbyRotationSpeed * Time.deltaTime;
                }
                yield return null;
            }
        }

        #endregion

    }
}
#endif