using Meta.WitAi.Attributes;
using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using Twinny.System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Twinny.UI
{
    public enum ButtonType
    {
        START,
        EXIT,
        SETTINGS,
        CHANGE_SCENE,
        NAVIGATION
    }

    [RequireComponent(typeof(PointableUnityEventWrapper))]

    public class ButtonAction : MonoBehaviour
    {
        public ButtonType type;
        [ShowIf("type!=ButtonType.EXIT")]
        public string scene;
        public int landMarkIndex;
        private PointableUnityEventWrapper _pointable;
        private void Awake()
        {
            _pointable = GetComponent<PointableUnityEventWrapper>();
            _pointable.WhenRelease.AddListener((pointerEvent) => OnRelease(pointerEvent));
            //_pointable.WhenRelease.AddListener(OnRelease);
        }

        public void OnRelease(PointerEvent pointerEvent)
        {


            switch (type)
            {
                case ButtonType.START:
                    LevelManager.Instance.StartExperience(scene);
                    break;
                case ButtonType.EXIT:
                    LevelManager.Instance.QuitExperience();
                    break;
                case ButtonType.CHANGE_SCENE:
                    LevelManager.Instance.RPC_ChangeScene(scene,landMarkIndex);
                    break;
                case ButtonType.NAVIGATION:
                    LevelManager.Instance.RPC_NavigateTo(landMarkIndex);
                    break;
                default:
                    break;
            }
        }

    }

}