using System;
using System.Reflection;
using Twinny.System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Twinny.UI
{

    [Serializable] public class OnLandscapeOrientation : UnityEvent { }
    [Serializable] public class OnPortraitOrientation : UnityEvent { }

    public class MobileHUDManager : HUDManager
    {
        public static new MobileHUDManager Instance { get => _instance as MobileHUDManager; }

        private ScreenOrientation _lastScreenOrientation;
        private bool _isLandscape;

        public delegate void onOrientationChanged(ScreenOrientation orientation);
        public static onOrientationChanged OnOrientationChanged;
        public OnLandscapeOrientation onLandscapeOrientation;
        public OnPortraitOrientation onPortraitOrientation;


        //        public delegate void onOrientationChanged(ScreenOrientation orientation);
        //        public static onOrientationChanged OnOrientationChanged;

        protected override void Start()
        {
            base.Start();
            _lastScreenOrientation = Screen.orientation;

#if UNITY_EDITOR
            _isLandscape = GetSize() == 6;
            ChangeOrientation(_isLandscape ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait);
#endif
        }

        protected override void Update()
        {
            base.Update();

            if (_lastScreenOrientation != Screen.orientation)
            {
                _lastScreenOrientation = Screen.orientation;
                ChangeOrientation(_lastScreenOrientation);
            }


#if UNITY_EDITOR
            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                _isLandscape = !_isLandscape;

                    SetSize(_isLandscape?6:5);
                    ChangeOrientation(_isLandscape ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait);
            }


#endif

        }

        private void ChangeOrientation(ScreenOrientation orientation)
        {
            OnOrientationChanged?.Invoke(orientation);
            if (orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown)
            {
                onPortraitOrientation?.Invoke();
            }
            else
            {
                onLandscapeOrientation?.Invoke();
            }
        }


        public static int GetSize()
        {
            var gvWndType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var gvWnd = EditorWindow.GetWindow(gvWndType);
            var size = selectedSizeIndexProp.GetValue(gvWnd, null);
            return (int)size;
        }

        public static void SetSize(int index)
        {
            var gvWndType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var gvWnd = EditorWindow.GetWindow(gvWndType);
            selectedSizeIndexProp.SetValue(gvWnd, index, null);

            Instance.ChangeOrientation((index == 6) ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait);


        }

#if UNITY_EDITOR
        // Menu para adicionar a resolução Landscape (1920x1080)
        [MenuItem("Twinny/Tools/Set GameView to Landscape (1920x1080)")]
        public static void SetLandscape()
        {
            SetSize(6);
            Instance.ChangeOrientation(ScreenOrientation.LandscapeLeft);
        }

        // Menu para adicionar a resolução Portrait (1080x1920)
        [MenuItem("Twinny/Tools/Set GameView to Portrait (1080x1920)")]
        public static void SetPortrait()
        {
            SetSize(5);
            Instance.ChangeOrientation(ScreenOrientation.Portrait);
        }

#endif
    }

}