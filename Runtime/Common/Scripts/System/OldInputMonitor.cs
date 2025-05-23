using Concept.Core;
using Concept.Helpers;
using Twinny.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Twinny.System
{
    public class OldInputMonitor : TSingleton<OldInputMonitor>
    {

        #region Properties

        private bool _isDragging = false;
        public static bool isDragging { get => Instance._isDragging; }

        private float _touchStartX = 0f;  // Posi��o X do toque inicial
        private float _touchStartY = 0f;  // Posi��o Y do toque inicial

        private float _initialPinchDistance = 0f;

        #endregion


        #region Delegates

        public delegate void onSelect(RaycastHit hit);
        public static onSelect OnSelect;

        public delegate void onTouch(float x, float y);
        public static onTouch OnTouch;

        public delegate void onRelease(float x, float y);
        public static onTouch OnRelease;

        public delegate void onCancelDrag(float x, float y);
        public static onTouch OnCancelDrag;


        #endregion

        #region MonoBehaviour Methods



        protected override void Update()
        {
            base.Update();
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.touchCount > 0)
            {
                Ray ray = default;
                RaycastHit hit;

                //Touch Actions
                if (Input.touchCount == 1)  //1 Finger Touch
                {
                    Touch touch = Input.GetTouch(0);  // Captura o primeiro toque
                    ray = Camera.main.ScreenPointToRay(touch.position);

                    switch (touch.phase)
                    {
                        case TouchPhase.Began:


                            _touchStartX = touch.position.x;
                            _touchStartY = touch.position.y;

                            CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnTouch(_touchStartX, _touchStartY));
                            OnTouch?.Invoke(_touchStartX, _touchStartY);
                            break;

                        case TouchPhase.Moved:

                            float deltaX = touch.position.x - _touchStartX;
                            float deltaY = touch.position.y - _touchStartY;
                            if (Mathf.Abs(deltaX) > 1.5f) //Avoid short movments
                            {
                                _isDragging = true;
                                CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnDraggingHorizontal(deltaX));
                            }
                            if (Mathf.Abs(deltaY) > 1.5f) //Avoid short movments
                            {
                                _isDragging = true;
                                CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnDraggingVertical(deltaY));
                            }


                            break;

                        case TouchPhase.Ended:
                            if (!_isDragging)
                            {
                                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                                {
                                    CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnSelect(hit.collider.gameObject));
                                    OnSelect?.Invoke(hit);
                                    return;
                                }
                                else
                                {
                                    CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnRelease(touch.position.x, touch.position.y));
                                    OnRelease?.Invoke(touch.position.x, touch.position.y);
                                }
                            }
                            else
                            {

                                _isDragging = false;
                                CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnDragEnded(touch.position.x, touch.position.y));
                                OnCancelDrag?.Invoke(touch.position.x, touch.position.y);
                            }
                            break;
                        case TouchPhase.Canceled:
                            if (_isDragging)
                            {
                                _isDragging = false;
                                CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnDragEnded(touch.position.x, touch.position.y));
                                OnCancelDrag?.Invoke(touch.position.x, touch.position.y);
                            }
                            break;
                    }

                }
                else if (Input.touchCount == 2) // 2 Fingers Touch
                {
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);

                    switch (touch1.phase)
                    {
                        case TouchPhase.Began:
                            _initialPinchDistance = Vector2.Distance(touch1.position, touch2.position);
                            CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnPinchingStart(_initialPinchDistance));
                            break;

                        case TouchPhase.Moved:
                            float currentDistance = Vector2.Distance(touch1.position, touch2.position);

                            if (currentDistance > 100f && Mathf.Abs(currentDistance - _initialPinchDistance) > 5f)
                            {
                                float pinchDelta = (currentDistance - _initialPinchDistance);
                                pinchDelta = Mathf.Clamp(pinchDelta, -0.1f, 0.1f);
                                CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnPinching(pinchDelta));
                            }

                            _initialPinchDistance = currentDistance;  // Update for next movment

                            break;
                    }
                }
                else if (Input.touchCount == 3) // 3 Fingers Touch NOT IMPLEMENTED YET
                {

                }



            }
            else
            {



                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 mousePosition = Input.mousePosition;
                    _touchStartX = mousePosition.x;
                    _touchStartY = mousePosition.y;
                    CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnTouch(_touchStartX, _touchStartY));
                    OnTouch?.Invoke(_touchStartX, _touchStartY);

                }

                // Mouse Actions
                if (Input.GetMouseButton(0))
                {
                    Vector2 mousePosition = Input.mousePosition;

                    float deltaX = mousePosition.x - _touchStartX;
                    float deltaY = mousePosition.y - _touchStartY;

                    //                Debug.LogWarning($"DELTA X:{deltaX} Y:{deltaY}");
                    if (Mathf.Abs(deltaX) > 1.5f)
                    {
                        if (!_isDragging) _isDragging = true;

                        CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnDraggingHorizontal(deltaX));
                    }
                    if (Mathf.Abs(deltaY) > 1.5f)
                    {
                        if (!_isDragging) _isDragging = true;

                        CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnDraggingVertical(deltaY));
                    }
                }
                else
                {

                }


                if (Input.GetMouseButtonUp(0))
                {
                    Vector2 mousePosition = Input.mousePosition;
                    Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                    RaycastHit hit;

                    if (!_isDragging)
                    {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnSelect(hit.collider.gameObject));
                            OnSelect?.Invoke(hit);
                            return;
                        }
                        else
                        {
                            CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnRelease(mousePosition.x, mousePosition.y));
                            OnRelease?.Invoke(mousePosition.x, mousePosition.y);

                        }
                    }
                    else
                    {
                        if (_isDragging)
                            _isDragging = false;
                        CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnDragEnded(mousePosition.x, mousePosition.y));
                        OnCancelDrag?.Invoke(mousePosition.x, mousePosition.y);
                    }

                }

                // Controle de zoom com sensibilidade ajustada
                float scrollInput = Input.GetAxis("Mouse ScrollWheel");
                if (scrollInput != 0)
                {

                    CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnPinching(scrollInput));

                }
            }

        }

        #endregion
    }
}
