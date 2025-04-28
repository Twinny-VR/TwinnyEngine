using Twinny.System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Twinny.UI
{

public class MobileHUDManager : HUDManager
{
        private ScreenOrientation _lastScreenOrientation;
        private bool _isLandscape;

        protected override void Start()
        {
            base.Start();
            _lastScreenOrientation = Screen.orientation;        }

        protected override void Update()
        {
            base.Update();

            if (_lastScreenOrientation != Screen.orientation) {
                _lastScreenOrientation = Screen.orientation;
                CallBackManager.CallAction<IUICallBacks>(callback => callback.OnOrientationChanged(_lastScreenOrientation));
            }


#if UNITY_EDITOR
            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                _isLandscape = !_isLandscape;
               
                CallBackManager.CallAction<IUICallBacks>(callback => callback.OnOrientationChanged(_isLandscape ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait));
            }
#endif

        }

        public override void OnOrientationChanged(ScreenOrientation orientation)
        {
            base.OnOrientationChanged(orientation);

                Debug.LogWarning(orientation);
        }
    }
    

}