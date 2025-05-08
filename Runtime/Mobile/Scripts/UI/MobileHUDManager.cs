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
       public class MobileHUDManager : HUDManager
    {
        [SerializeField] private GameObject BT_Immersive;

        public static new MobileHUDManager Instance { get => _instance as MobileHUDManager; }

        private ScreenOrientation _lastScreenOrientation;

        //        public delegate void onOrientationChanged(ScreenOrientation orientation);
        //        public static onOrientationChanged OnOrientationChanged;

        protected override void Start()
        {
            base.Start();
            _lastScreenOrientation = Screen.orientation;

        }

        protected override void Update()
        {
            base.Update();

            if (_lastScreenOrientation != Screen.orientation)
            {
                _lastScreenOrientation = Screen.orientation;
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


        #region UI Callbacks

        public override void OnLoadScene()
        {
            base.OnLoadScene();
            if (MobileSceneFeature.Instance)
            {
                BT_Immersive.SetActive(MobileLevelManager.currentInterest.allowFirstPerson);
                if (_animator) _animator.SetBool("retracted", false);
                
            }
        }

        #endregion

    }

}