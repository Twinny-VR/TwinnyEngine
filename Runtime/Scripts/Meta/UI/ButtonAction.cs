#if OCULUS
using Oculus.Interaction;
using Twinny.XR;
#endif

using Twinny.Helpers;
using Twinny.Localization;
using Twinny.System;
using UnityEngine;
using UnityEngine.UI;


namespace Twinny.UI
{
    public enum ButtonType
    {
        START,
        EXIT,
        SETTINGS,
        CHANGE_SCENE,
        NAVIGATION,
        ACTION,
        RESET
    }

#if OCULUS
    [RequireComponent(typeof(PointableUnityEventWrapper))]
#endif
    public class ButtonAction : MonoBehaviour
    {
        public ButtonType type;
#if OCULUS
public string name;
        [SerializeField] private PointableUnityEventWrapper _pointable;
#else
        [SerializeField] private Button _button;
#endif
#if UNITY_EDITOR
        [HideInInspector] public bool showParameter;
        [ShowIf("showParameter")]
#endif
        public string parameter;
#if UNITY_EDITOR
        [HideInInspector] public bool showIndex;
        [ShowIf("showIndex")]
#endif
        public int landMarkIndex;


        #region MonoBehaviour Methods
#if UNITY_EDITOR

        private void OnValidate() 
        {
            showParameter = type != ButtonType.EXIT && type != ButtonType.RESET && type != ButtonType.NAVIGATION;
            showIndex = type == ButtonType.START || type == ButtonType.NAVIGATION || type == ButtonType.CHANGE_SCENE;
        }
#endif
        protected virtual void Awake()
        {
#if OCULUS
            if(!_pointable)
            _pointable = GetComponent<PointableUnityEventWrapper>();
            _pointable.WhenRelease.AddListener((pointerEvent) => OnRelease());
#else
            if (!_button)
                _button = GetComponent<Button>();
            _button.onClick.AddListener(() => OnRelease());
#endif
        }


        #endregion
        [ContextMenu("CLICK")]
        public void OnRelease()
        {

#if OCULUS
            //TODO Criar um sistema de configurações
            if (!LevelManagerXR.Config.allowClickSafeAreaOutside && !AnchorManager.Instance.isInSafeArea)
            {
                AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%BACK_TO_SAFE_AREA"), AlertViewHUD.MessageType.Warning, 5f);
                return;
            }
#endif
            switch (type)
            {
#if OCULUS
                case ButtonType.START:
                    LevelManagerXR.StartExperience(parameter, landMarkIndex);
                    break;
                case ButtonType.RESET:
                    LevelManagerXR.QuitExperience();
                    break;
                case ButtonType.CHANGE_SCENE:
                        LevelManagerXR.instance.RPC_ChangeScene(parameter, landMarkIndex);
                    break;
                case ButtonType.NAVIGATION:
                    LevelManagerXR.instance.RPC_NavigateTo(landMarkIndex);
                    break;
#else //If Multiplatform
                case ButtonType.START:
                    break;
                case ButtonType.SETTINGS:
                    break;
                case ButtonType.CHANGE_SCENE:
                    _ = LevelManager.Instance.ChangeScene(parameter, landMarkIndex);
                    break;
                case ButtonType.NAVIGATION:
                    LevelManager.Instance.NavigateTo(landMarkIndex);
                    break;
                case ButtonType.RESET:
                    _ = LevelManager.Instance.ResetExperience();
                    break;
                case ButtonType.EXIT:
                    Application.Quit();
                    break;
#endif
                case ButtonType.ACTION:
                    ActionManager.CallAction(parameter);
                    break;
            }
        }

    }

}