#if !OCULUS
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using static Twinny.System.Cameras.CameraManager;
using UnityEngine.EventSystems;
using Twinny.UI;
using System;

namespace Twinny.System.Cameras
{
    public class CameraHandler : MonoBehaviour, IInputCallBacks
    {
        #region Properties

        [SerializeField] CameraState cameraType;
        private CinemachineVirtualCameraBase _virtualCamera;

        private CinemachineCameraOffset _offset;
        private CinemachinePOV _pov;
        private CinemachineOrbitalTransposer _transposer;

        [SerializeField]
        private InterestItem _interestItem;

        private float _initialX = 0f;  // O valor inicial antes de come�ar o arrasto
        private float _initialY = .5f;  // O valor inicial antes de come�ar o arrasto


        public float _xAxis = 0f;  // O valor que vai ser alterado com o arrasto
        public float _yAxis = .5f;  // O valor que vai ser alterado com o arrasto


        private float _initialYaw = 0f;
        private float _initialZoom = 0;
        private float _zoom = 0;
        public float zoom { get => _zoom; set => _zoom = value; }

        public float fov { get =>  (_virtualCamera as CinemachineVirtualCamera) .m_Lens.FieldOfView; set { (_virtualCamera as CinemachineVirtualCamera).m_Lens.FieldOfView = value; } }
        public Transform follow { get => _virtualCamera.Follow; set { _virtualCamera.Follow = value; } }
        public Transform lookAt { get => _virtualCamera.LookAt; set { _virtualCamera.LookAt = value; } }
        public int priority { get => _virtualCamera.Priority; set { _virtualCamera.Priority = value; } }

        private Coroutine _recenterCor;
        private Coroutine _standbyCor;

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
                case CameraState.FPS:
                    fov = config.fpsDesiredFov; break;
                case CameraState.PAN:
                    fov = config.desiredFov; break;
                case CameraState.THIRD:
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
            _interestItem = _virtualCamera.Follow.GetComponent<InterestItem>();
            switch (cameraType)
            {
                case CameraState.FPS:
                    _zoom = _initialYaw = 0;
                    _pov.m_VerticalAxis.Value = _initialYaw;
                    break;
                case CameraState.LOCKED:
                case CameraState.PAN:
                    _zoom = _initialZoom = _interestItem.zoomMin;
                    _initialYaw = _interestItem.yawRange.y / 2;
                    _transposer.m_FollowOffset.y = _initialYaw;
                    _offset.m_Offset.z = _zoom;
                    break;

                case CameraState.THIRD:

                    _zoom = _initialZoom = config.thirdZoomLimitMin;

                    if (_pov)
                    {
                        _initialYaw =0;
                        _pov.m_VerticalAxis.Value = _pov.m_VerticalAxis.m_MaxValue / 2;
                    }
                    else
                        Debug.LogError($"[CameraHandler] POV is missing in {name} game object.");


                    if (_offset)
                        _offset.m_Offset.z = _zoom;
                    else
                        Debug.LogError($"[CameraHandler] Offset is missing in {name} game object.");

                    break;
                default:
                    break;
            }

            priority = 11;
            enabled = true;

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

            _xAxis = _initialX + value * config.horizontalSensitivity;

            switch (cameraType)
            {
                case CameraState.FPS:
                case CameraState.THIRD:
                    if (_pov)
                        _pov.m_HorizontalAxis.Value = _xAxis;
                    else
                        Debug.LogError($"[CameraHandler] POV is missing in {name} game object.");
                    break;
                case CameraState.LOCKED:
                case CameraState.PAN:
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
                case CameraState.FPS:
                case CameraState.THIRD:
                    _yAxis = _initialY + value * config.verticalSensitivity;
                    if (_pov)
                        _pov.m_VerticalAxis.Value = _yAxis;
                    else
                        Debug.LogError($"[CameraHandler] POV is missing in {name} game object.");
                    break;
                case CameraState.LOCKED:
                case CameraState.PAN:
                    _yAxis = _initialY + value * -config.verticalSensitivity * _interestItem.yaySpeedMultiply;
                    if (_transposer)
                    {

                        _yAxis = Mathf.Clamp(_yAxis, _interestItem.yawRange.x, _interestItem.yawRange.y);

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
                case CameraState.FPS: //No zoom for FPS Camera
                    if (value < 0)
                        SwitchCameraState(CameraState.THIRD);
                    return;
                case CameraState.LOCKED:
                case CameraState.PAN:



                    if (_interestItem)
                    {
                        _zoom += value * _interestItem.zoomSensitivity * 100f * _interestItem.zoomSpeedMultiply;
                        _zoom = Mathf.Clamp(_zoom, _interestItem.zoomMin, _interestItem.zoomMax);

                    }
                    else
                        Debug.LogError($"[CameraHandler] Subject Interest is missing in {name} game object.");

                    if (_offset)
                    {
                        _offset.m_Offset.z = _zoom;
                    }
                    else
                        Debug.LogError($"[CameraHandler] Offset is missing in {name} game object.");


                    break;
                case CameraState.THIRD:
                    _zoom += value * config.zoomSensitivity * 100f;
                    _zoom = Mathf.Clamp(_zoom, config.thirdZoomLimitMin, config.thirdZoomLimitMax);

                    if (_offset)
                    {
                        if (_zoom >= config.thirdZoomLimitMax)
                            SwitchCameraState(CameraState.FPS);

                        _offset.m_Offset.z = _zoom;
                    }
                    else
                        Debug.LogError($"[CameraHandler] Offset is missing in {name} game object.");

                    break;
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
                case CameraState.FPS:
                case CameraState.THIRD:
                    if (_pov)
                    {
                        _initialX = _pov.m_HorizontalAxis.Value;
                        _initialY = _pov.m_VerticalAxis.Value;
                    }
                    else
                        Debug.LogError($"[CameraHandler] POV is missing in {name} game object.");
                    break;
                case CameraState.LOCKED:
                case CameraState.PAN:
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

            while (elapsed < duration) {

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (_pov)
                {
                    _pov.m_VerticalAxis.Value = Mathf.Lerp(startY, _initialYaw, t);
                }else
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
                }else
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