using Concept.Helpers;
using Twinny.System;
using UnityEngine;
using UnityEngine.UI;



namespace Twinny.UI
{
    public enum ButtonType
    {
        START,
        QUIT,
        SETTINGS,
        CHANGE_SCENE,
        NAVIGATION,
        ACTION,
        RESET
    }
    public class ButtonAction : MonoBehaviour
    {
        public ButtonType type;
        [SerializeField] private Button _button;
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
            showParameter = type != ButtonType.QUIT && type != ButtonType.RESET && type != ButtonType.NAVIGATION;
            showIndex = type == ButtonType.START || type == ButtonType.NAVIGATION || type == ButtonType.CHANGE_SCENE;
        }
#endif
        protected virtual void Awake()
        {
            if (!_button)
                _button = GetComponent<Button>();
            _button.onClick.AddListener(() => OnRelease());
        }


        #endregion
        [ContextMenu("CLICK")]
        public void OnRelease()
        {

            if (CanvasTransition.isTransitioning) 
                return;
            switch (type)
            {
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
                case ButtonType.QUIT:
                    Application.Quit();
                    break;
                case ButtonType.ACTION:
                    ActionManager.CallAction(parameter);
                    break;
            }
        }

    }
}