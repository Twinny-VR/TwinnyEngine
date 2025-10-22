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
            EditorGUI.BeginProperty(position, label, property);

            // Linha principal — campo de referência do ScriptableObject
            Rect fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(fieldRect, property, label, true);

            // Se ainda não há ScriptableObject atribuído
            if (property.objectReferenceValue == null)
            {
                // Cria botão pra criar um novo ScriptableObject
                Rect buttonRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

                if (GUI.Button(buttonRect, $"Criar novo {fieldInfo.FieldType.Name}"))
                {
                    var asset = ScriptableObject.CreateInstance(fieldInfo.FieldType);
                    string path = "Assets/New " + fieldInfo.FieldType.Name + ".asset";
                    path = AssetDatabase.GenerateUniqueAssetPath(path);

                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();

                    property.objectReferenceValue = asset;
                    property.serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.EndProperty();
                return;
            }

            // Se tem ScriptableObject, mostra o foldout
            Rect foldoutRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 4, position.width, EditorGUIUtility.singleLineHeight);
            isFoldedOut = EditorGUI.Foldout(foldoutRect, isFoldedOut, "Properties", true);

            if (isFoldedOut)
            {
                SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
                SerializedProperty iterator = serializedObject.GetIterator();

                float yOffset = foldoutRect.y + EditorGUIUtility.singleLineHeight + 2;

                iterator.NextVisible(true); // pula o campo script
                while (iterator.NextVisible(false))
                {
                    Rect propRect = new Rect(position.x + 15, yOffset, position.width - 15, EditorGUI.GetPropertyHeight(iterator, true));
                    EditorGUI.PropertyField(propRect, iterator, true);
                    yOffset += EditorGUI.GetPropertyHeight(iterator, true) + 2;
                }

                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue != null)
            {
                float height = EditorGUIUtility.singleLineHeight * 2f; // campo + foldout
                if (isFoldedOut)
                {
                    SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
                    SerializedProperty iterator = serializedObject.GetIterator();

                    while (iterator.NextVisible(true))
                    {
                        height += EditorGUI.GetPropertyHeight(iterator, true) + 2f;
                    }

                    height += 8f; // espaço extra no final
                }

                return height;
            }
            else
            {
                // Campo em branco + botão + espaçamento extra
                return EditorGUIUtility.singleLineHeight * 2f + 8f; // 1 linha do campo + 1 do botão + padding
            }
        }




    }


}
#endif



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