
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Twinny.UI
{


[CustomEditor(typeof(ResponsiveGridLayoutGroup))]
[CanEditMultipleObjects]
public class ResponsiveGridLayoutGroupEditor : UnityEditor.Editor
{
    private HashSet<string> subclassFieldNames;

    void OnEnable()
    {
        var type = target.GetType();
        subclassFieldNames = new HashSet<string>(
            type
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null)
                .Select(f => f.Name)
        );
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty prop = serializedObject.GetIterator();
        prop.NextVisible(true); // pular o script (m_Script)

        while (prop.NextVisible(false))
        {
            if (subclassFieldNames.Contains(prop.name))
            {
                EditorGUILayout.PropertyField(prop, true);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}

}

/*

using Twinny.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResponsiveGridLayoutGroup), true)]
[CanEditMultipleObjects]
public class ResponsiveGridLayoutGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            // Evita desenhar o campo "m_Script" (readonly e interno do Unity)
            if (prop.name == "m_Script")
                continue;

            EditorGUILayout.PropertyField(prop, true);
            enterChildren = false;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
*/