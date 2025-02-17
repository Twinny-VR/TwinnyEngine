#if OCULUS
using Meta.WitAi.Attributes;
using Oculus.Interaction;
using Twinny.XR;
#endif

using Twinny.Helpers;
using Twinny.Localization;
using Twinny.System;
using UnityEngine;


namespace Twinny.UI
{
    public enum ButtonType
    {
        START,
        EXIT,
        SETTINGS,
        CHANGE_SCENE,
        NAVIGATION,
        ACTION
    }

#if OCULUS
    [RequireComponent(typeof(PointableUnityEventWrapper))]
#endif
    public class ButtonAction : MonoBehaviour
    {
        public ButtonType type;
#if OCULUS
        [ShowIf("type!=ButtonType.EXIT")]
        private PointableUnityEventWrapper _pointable;
#endif
        public string parameter;
        public int landMarkIndex;
        protected virtual void Awake()
        {
#if OCULUS
            _pointable = GetComponent<PointableUnityEventWrapper>();
            _pointable.WhenRelease.AddListener((pointerEvent) => OnRelease());
#endif
        }

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

            switch (type)
            {
                case ButtonType.START:
                    LevelManagerXR.StartExperience(parameter, landMarkIndex);
                    break;
                case ButtonType.EXIT:
                    LevelManagerXR.QuitExperience();
                    break;
                case ButtonType.CHANGE_SCENE:
                        LevelManagerXR.instance.RPC_ChangeScene(parameter, landMarkIndex);
                    break;
                case ButtonType.NAVIGATION:
                    LevelManagerXR.instance.RPC_NavigateTo(landMarkIndex);
                    break;
                case ButtonType.ACTION:
                    ActionManager.CallAction(parameter);
                    break;
                default:
                    break;
            }
#endif
        }

    }

}