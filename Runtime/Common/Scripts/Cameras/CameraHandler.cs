using Concept.Core;
using Twinny.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace Twinny.System.Cameras
{

    public class CameraHandler : MonoBehaviour, ICameraCallBacks, IInputCallBacks
    {

        static CameraHandler _instance;
        public static CameraHandler Instance
        {
            get => _instance; set
            {
                _instance = value;
            }
        }

        [SerializeField] private CinemachineCamera _camera;
        private CinemachineOrbitalFollow _orbital;
        private CinemachinePanTilt _panTilt;
        private CinemachineCameraOffset _offset;

        private float _initialX = 0f;
        private float _initialY = .5f;

        private float _xAxis = 0f;
        private float _yAxis = .5f;

        private float _zoom = 0;
        [SerializeField] private float _zoomSpeedMultiply = 1f;
        [VectorLabels("Min", "Max")]
        [SerializeField] private Vector2 _zoomRange = new Vector2(0f, 100f);
        [SerializeField] private bool _allowFirstPerson;
        public bool allowFirstPerson { get => _allowFirstPerson; }
        private void OnEnable()
        {
            CallbackHub.RegisterCallback<ICameraCallBacks>(this);
            CallbackHub.RegisterCallback<IInputCallBacks>(this);
        }

        private void OnDisable()
        {
            CallbackHub.UnregisterCallback<ICameraCallBacks>(this);
            CallbackHub.UnregisterCallback<IInputCallBacks>(this);

        }
        private void Awake()
        {
            if (!_camera) _camera = GetComponent<CinemachineCamera>();
            _orbital = _camera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineOrbitalFollow;
            _panTilt = _camera.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachinePanTilt;
            _offset = _camera.GetComponent<CinemachineCameraOffset>();
            //if (!_instance)
                _instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

            //_camera.Priority = _state == CameraState.LIVE ? 10 : 0;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public static void SetHorizontal(float value)
        {
        }
        public static void SetVertical(float value)
        {

        }

        #region Camera Callback Methods

        public void OnChangeCamera(CameraHandler camera) 
        {
            _camera.Priority = camera == this ? 10 : 0;
            if (camera == this)
            {
                _instance = this;
            }
        }

        public void OnTouch(float x, float y)
        {
            if (_panTilt && _orbital)
            {

                _initialX = _orbital.HorizontalAxis.Value;
                _initialY = _panTilt.TiltAxis.Value;
            }
            else
            if (_panTilt)
            {
                _initialX = _panTilt.PanAxis.Value;
                _initialY = _panTilt.TiltAxis.Value;
            }
            else
            if (_orbital)
            {
                _initialX = _orbital.HorizontalAxis.Value;
                _initialY = _orbital.VerticalAxis.Value;
            }

        }

        public void OnRelease(float x, float y)
        {
        }

        public void OnDraggingHorizontal(float factor)
        {
            if (!_instance || _instance != this) return;

            _xAxis = _initialX + factor * -CameraManager.config.sesitivity.x;



            if (_panTilt)
                _panTilt.PanAxis.Value = _xAxis;

            if (_orbital)
                _orbital.HorizontalAxis.Value = _xAxis;
        }

        public void OnDraggingVertical(float factor)
        {
            if (!_instance || _instance != this) return;

            float value = _initialY + factor * CameraManager.config.sesitivity.y;

            if (_panTilt)
            {
                _yAxis = Mathf.Clamp(value, _panTilt.TiltAxis.Range.x, _panTilt.TiltAxis.Range.y);
                _panTilt.TiltAxis.Value = _yAxis;
            }

            if (_orbital)
            {
                _yAxis = Mathf.Clamp(value, _orbital.VerticalAxis.Range.x, _orbital.VerticalAxis.Range.y);
                _orbital.VerticalAxis.Value = _yAxis;

            }
        }

        public void OnDragEnded(float x, float y)
        {
        }

        public void OnThreeFingersDragging(float x, float y)
        {
        }

        public void OnPinchingStart(float factor)
        {
        }

        public void OnPinching(float factor)
        {
            if (!_instance || _instance != this || !_offset) return;


            _zoom += factor * CameraManager.config.sesitivity.z * 100f * _zoomSpeedMultiply;
            _zoom = Mathf.Clamp(_zoom, _zoomRange.x, _zoomRange.y);
            if (_allowFirstPerson && _zoom >= _zoomRange.y) FirstPersonAgent.TakeControl(true); 
            _offset.Offset.z = _zoom;
        }

        public void OnSelect(object sender)
        {
        }

        #endregion
    }
}
