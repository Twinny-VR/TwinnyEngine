#if UNITY_EDITOR
using Twinny.UI;
using UnityEditor;
using UnityEngine;

namespace Twinny.Editor
{
    [CustomPropertyDrawer(typeof(DrawScriptableAttribute))]
    public class ScriptableObjectDrawer : PropertyDrawer
    {

        private bool isFoldedOut = false; // Variável que controla o estado do foldout

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Se o campo for uma referência de objeto
            if (property.objectReferenceValue != null)
            {
                // Desenha o campo de referência do ScriptableObject (como uma referência normal)
                EditorGUI.PropertyField(position, property, label, true);

                // Criando um "foldout" para controlar a expansão/colapso
                Rect foldoutRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
                isFoldedOut = EditorGUI.Foldout(foldoutRect, isFoldedOut, "Properties", true);

                if (isFoldedOut)
                {
                    // Cria um SerializedObject para o ScriptableObject
                    SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);

                    // Itera sobre as propriedades do ScriptableObject e as desenha no Inspector
                    SerializedProperty iterator = serializedObject.GetIterator();
                    float yOffset = foldoutRect.y + EditorGUIUtility.singleLineHeight + 2; // Move para a próxima linha

                    // Desenha as propriedades visíveis do ScriptableObject
                    while (iterator.NextVisible(true))
                    {
                        Rect fieldRect = new Rect(position.x, yOffset, position.width, EditorGUI.GetPropertyHeight(iterator, true));
                        EditorGUI.PropertyField(fieldRect, iterator, true);
                        yOffset += EditorGUI.GetPropertyHeight(iterator, true) + 2; // Adiciona espaçamento entre as propriedades
                    }
                }
            }
            else
            {
                // Se não houver ScriptableObject, mostra uma mensagem
                EditorGUI.LabelField(position, "No ScriptableObject assigned.");
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Se o campo for uma referência a um ScriptableObject, calcula a altura
            if (property.objectReferenceValue != null)
            {
                float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.singleLineHeight; // Uma linha para a referência + uma linha para o foldout
                if (isFoldedOut)
                {
                    SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
                    SerializedProperty iterator = serializedObject.GetIterator();
                    while (iterator.NextVisible(true))
                    {
                        height += EditorGUI.GetPropertyHeight(iterator, true) + 2; // Altura das propriedades do ScriptableObject
                    }
                }
                return height;
            }
            return EditorGUIUtility.singleLineHeight; // Caso não haja ScriptableObject, apenas a altura do campo de referência
        }


    }


    /*

    [CustomPropertyDrawer(typeof(CameraRuntime))]
    public class CameraRuntimePropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Impede o Unity de desenhar o campo de "Script"
            if (property.objectReferenceValue != null)
            {
                // Desenha o campo do CameraRuntime (evitando o "Script" redundante)
                EditorGUI.PropertyField(position, property, label, true);

                // Cria um SerializedObject para o CameraRuntime
                SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);

                // Itera sobre as propriedades do CameraRuntime e as desenha no Inspector
                SerializedProperty iterator = serializedObject.GetIterator();
                float yOffset = position.y + EditorGUIUtility.singleLineHeight + 2; // Move para a próxima linha

                while (iterator.NextVisible(true))
                {
                    // Desenha as propriedades do CameraRuntime
                    Rect fieldRect = new Rect(position.x, yOffset, position.width, EditorGUI.GetPropertyHeight(iterator, true));
                    EditorGUI.PropertyField(fieldRect, iterator, true);
                    yOffset += EditorGUI.GetPropertyHeight(iterator, true) + 2; // Adiciona espaçamento entre as propriedades
                }
            }
            else
            {
                // Caso não haja um CameraRuntime, exibe uma mensagem
                EditorGUI.LabelField(position, "No CameraRuntime assigned.");
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Calcula a altura necessária para desenhar o campo e as propriedades do ScriptableObject
            if (property.objectReferenceValue != null)
            {
                float height = EditorGUIUtility.singleLineHeight;
                SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
                SerializedProperty iterator = serializedObject.GetIterator();
                while (iterator.NextVisible(true))
                {
                    height += EditorGUI.GetPropertyHeight(iterator, true) + 2; // Altura das propriedades do CameraRuntime
                }
                return height;
            }
            return EditorGUIUtility.singleLineHeight; // Caso não tenha CameraRuntime, apenas a altura do campo de referência
        }

    }
    */
}
#endif