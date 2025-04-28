#if UNITY_EDITOR
using Twinny.UI;
using UnityEditor;
using UnityEngine;

#if TWINNY_OPENXR
[CustomEditor(typeof(ButtonActionXR))]
public class ButtonActionXREditor : Editor
{
    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        ButtonActionXR buttonActionXR = (ButtonActionXR)target;
        if (EditorApplication.isPlaying)
            if (GUILayout.Button("CLICK"))
        {
            buttonActionXR.OnRelease();
        }


    }
}
#endif

#if TWINNY_WIN
[CustomEditor(typeof(ButtonAction))]
public class ButtonActionEditor : Editor
{
    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        ButtonAction buttonAction = (ButtonAction)target;
        if (EditorApplication.isPlaying)
            if (GUILayout.Button("CLICK"))
            {
                buttonAction.OnRelease();
            }


    }
}
#endif
#endif