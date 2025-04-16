using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Twinny.System
{
    public class InputMonitor : TSingleton<InputMonitor>
    {
        #region Properties

        private bool _isDragging = false;
        public static bool isDragging => Instance._isDragging;

        private Vector2 _touchStartPos;
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

        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            // === TOUCH INPUT ===
            if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            {
                var touches = Touchscreen.current.touches;

                if (touches.Count == 1)
                {
                    var touch = touches[0];
                    var phase = touch.phase.ReadValue();
                    var pos = touch.position.ReadValue();

                    Ray ray = Camera.main.ScreenPointToRay(pos);
                    RaycastHit hit;

                    switch (phase)
                    {
                        case UnityEngine.InputSystem.TouchPhase.Began:
                            _touchStartPos = pos;
                            CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnTouch(pos.x, pos.y));
                            OnTouch?.Invoke(pos.x, pos.y);
                            break;

                        case UnityEngine.InputSystem.TouchPhase.Moved:
                            float deltaX = pos.x - _touchStartPos.x;
                            float deltaY = pos.y - _touchStartPos.y;

                            if (Mathf.Abs(deltaX) > 1.5f)
                            {
                                _isDragging = true;
                                CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnDraggingHorizontal(deltaX));
                            }

                            if (Mathf.Abs(deltaY) > 1.5f)
                            {
                                _isDragging = true;
                                CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnDraggingVertical(deltaY));
                            }
                            break;

                        case UnityEngine.InputSystem.TouchPhase.Ended:
                            if (!_isDragging)
                            {
                                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                                {
                                    CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnSelect(hit.collider.gameObject));
                                    OnSelect?.Invoke(hit);
                                }
                                else
                                {
                                    CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnRelease(pos.x, pos.y));
                                    OnRelease?.Invoke(pos.x, pos.y);
                                }
                            }
                            else
                            {
                                _isDragging = false;
                                CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnDragEnded(pos.x, pos.y));
                                OnCancelDrag?.Invoke(pos.x, pos.y);
                            }
                            break;
                    }
                }
                else if (touches.Count == 2)
                {
                    var touch1 = touches[0];
                    var touch2 = touches[1];

                    var pos1 = touch1.position.ReadValue();
                    var pos2 = touch2.position.ReadValue();

                    float dist = Vector2.Distance(pos1, pos2);
                    var phase1 = touch1.phase.ReadValue();

                    if (phase1 == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        _initialPinchDistance = dist;
                        CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnPinchingStart(_initialPinchDistance));
                    }
                    else if (phase1 == UnityEngine.InputSystem.TouchPhase.Moved)
                    {
                        float pinchDelta = dist - _initialPinchDistance;
                        if (Mathf.Abs(pinchDelta) > 5f)
                        {
                            pinchDelta = Mathf.Clamp(pinchDelta * 0.01f, -0.1f, 0.1f);
                            CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnPinching(pinchDelta));
                        }

                        _initialPinchDistance = dist;
                    }
                }

                return; // Se for touch, ignora mouse abaixo
            }

            // === MOUSE INPUT ===
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _touchStartPos = mousePos;
                    CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnTouch(mousePos.x, mousePos.y));
                    OnTouch?.Invoke(mousePos.x, mousePos.y);
                }

                if (Mouse.current.leftButton.isPressed)
                {
                    float deltaX = mousePos.x - _touchStartPos.x;
                    float deltaY = mousePos.y - _touchStartPos.y;

                    if (Mathf.Abs(deltaX) > 1.5f)
                    {
                        _isDragging = true;
                        CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnDraggingHorizontal(deltaX));
                    }

                    if (Mathf.Abs(deltaY) > 1.5f)
                    {
                        _isDragging = true;
                        CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnDraggingVertical(deltaY));
                    }
                }

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    Ray ray = Camera.main.ScreenPointToRay(mousePos);
                    RaycastHit hit;

                    if (!_isDragging)
                    {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnSelect(hit.collider.gameObject));
                            OnSelect?.Invoke(hit);
                        }
                        else
                        {
                            CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnRelease(mousePos.x, mousePos.y));
                            OnRelease?.Invoke(mousePos.x, mousePos.y);
                        }
                    }
                    else
                    {
                        _isDragging = false;
                        CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnDragEnded(mousePos.x, mousePos.y));
                        OnCancelDrag?.Invoke(mousePos.x, mousePos.y);
                    }
                }

                float scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    CallBackUI.CallAction<IInputCallBacks>(cb => cb.OnPinching(scroll * 0.01f));
                }
            }
        }
    }
}
