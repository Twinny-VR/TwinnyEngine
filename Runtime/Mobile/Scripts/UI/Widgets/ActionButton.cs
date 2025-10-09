using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ActionButton : VisualElement
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

}
