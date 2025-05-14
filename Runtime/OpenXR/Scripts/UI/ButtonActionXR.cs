using Oculus.Interaction;
using Twinny.XR;

using Twinny.Helpers;
using Twinny.Localization;
using Twinny.System;
using UnityEngine;
using Concept.Helpers;



namespace Twinny.UI
{

    [RequireComponent(typeof(PointableUnityEventWrapper))]
    public class ButtonActionXR : MonoBehaviour
    {
        public ButtonType type;
        [SerializeField] private PointableUnityEventWrapper _pointable;
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
            if(!_pointable)
            _pointable = GetComponent<PointableUnityEventWrapper>();
            _pointable.WhenRelease.AddListener((pointerEvent) => OnRelease());
        }


        #endregion
        //[ContextMenu("CLICK")]

        public void OnRelease()
        {
            //TODO Criar um sistema de configurações
            if (!(TwinnyManager.config as RuntimeXR).allowClickSafeAreaOutside && !AnchorManager.Instance.isInSafeArea)
            {
                AlertViewHUD.PostMessage(LocalizationProvider.GetTranslated("%BACK_TO_SAFE_AREA"), AlertViewHUD.MessageType.Warning, 5f);
                return;
            }
            switch (type)
            {
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
                case ButtonType.ACTION:
                    ActionManager.CallAction(parameter);
                    break;
            }
        }

    }

}