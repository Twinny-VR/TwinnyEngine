#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using Twinny.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Twinny.Editor
{

    /// <summary>
    /// A drawer to handle drawing fields with a [HideIf] attribute. When fields have this attribute they will be hidden
    /// in the inspector conditionally based on the evaluation of a field or property.
    /// </summary>
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfDrawer : PropertyDrawer
    {
        private bool IsVisible(SerializedProperty property)
        {
            HideIfAttribute hideIf = attribute as HideIfAttribute;
            SerializedProperty conditionProperty =
                property.GetParent()?.FindPropertyRelative(hideIf.condition);
            // If it wasn't found relative to the property, check siblings.
            if (null == conditionProperty)
            {
                conditionProperty = property.serializedObject.FindProperty(hideIf.condition);
            }

            if (conditionProperty != null)
            {
                if (conditionProperty.type == "bool") return !conditionProperty.boolValue;
                return conditionProperty.objectReferenceValue == null;
            }

            return true;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsVisible(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsVisible(property))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return 0f;
        }
    }



    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfAttributeDrawer : PropertyDrawer
    {
        private bool IsVisible(SerializedProperty property)
        {
            ShowIfAttribute hideIf = attribute as ShowIfAttribute;
            SerializedProperty conditionProperty =
                property.FindPropertyRelative(hideIf.condition);
            //property.serializedObject.FindProperty(hideIf.condition);
            // If it wasn't found relative to the property, check siblings.
            if (null == conditionProperty)
            {
                conditionProperty = property.serializedObject.FindProperty(hideIf.condition);
            }

            if (conditionProperty != null)
            {
                if (conditionProperty.type == "bool") return !conditionProperty.boolValue;
                return conditionProperty.objectReferenceValue == null;
            }

            return true;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsVisible(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!IsVisible(property))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return 0f;
        }


    }
    /*
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {


            var showIfAttribute = (ShowIfAttribute)attribute;

            var conditionProperty = property.serializedObject.FindProperty(showIfAttribute.condition);

            if (conditionProperty != null)
            {
                // Se for do tipo bool
                if (conditionProperty.propertyType == SerializedPropertyType.Boolean)
                {
                    if (!conditionProperty.boolValue)
                    {
                        return;  // Não desenha a propriedade se a condição booleana for falsa
                    }
                }
            }

            // Se for um objeto ou struct, precisamos "expandir" o conteúdo
            if (property.propertyType == SerializedPropertyType.ObjectReference || property.propertyType == SerializedPropertyType.Generic)
            {
                Debug.LogWarning($"{property.name} StartY: {position.y}");

                Color originalColor = GUI.color;

                GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };

                GUI.Label(new Rect(position.x, position.y, position.width, 16f), property.displayName + ":", boldLabelStyle); // Aplica a label com o estilo
                position.y += 16f;

                var y = DrawStructFields(position, property);


                Debug.LogWarning($"{property.name} Height: {y}");
                position.y = y + 16f;

                Debug.LogWarning($"{property.name} FinalY: {position.y}");

            }
            else
            {
                // Caso a propriedade não seja um objeto, desenha normalmente
                EditorGUI.PropertyField(position, property, label);
                position.y += EditorGUI.GetPropertyHeight(property, true);
            }


            // Se a condição for atendida, desenha a propriedade normalmente
            //  EditorGUI.PropertyField(position, property, label);
        }


        private float DrawStructFields(Rect position, SerializedProperty property)
        {
            // Desenha os campos internos de uma struct
            if (property.hasChildren)
            {
                SerializedProperty iterator = property.Copy();
                iterator.NextVisible(true);  // Avança para o primeiro campo

                bool first = true;

                // Itera por todos os campos visíveis e os desenha
                while (iterator.NextVisible(first))
                {
                    first = false;

                    Rect newPosition = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(iterator, true));
                    EditorGUI.PropertyField(newPosition, iterator, true);
                    position.y += 100f;
                }

            }
            return position.y;
        }

    }
    */


    [CustomPropertyDrawer(typeof(CustomButtonAttribute))]
    public class CustomButtonPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label); // Desenha o campo padrão (se houver)

            // Verifique se o campo é uma string que contém o nome do método
            if (property.propertyType == SerializedPropertyType.String)
            {
                string methodName = property.stringValue;

                // Desenha o botão no Inspector
                if (GUI.Button(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, 20), "Clique para Ação"))
                {
                    var targetObject = property.serializedObject.targetObject;
                    var method = targetObject.GetType().GetMethod(methodName);

                    if (method != null)
                    {
                        // Chama o método
                        method.Invoke(targetObject, null);
                    }
                    else
                    {
                        Debug.LogWarning("Método não encontrado: " + methodName);
                    }
                }
            }
        }

    }



    [CustomPropertyDrawer(typeof(SubPanelAttribute))]
    public class SubPanelPropertyDrawer : PropertyDrawer
    {



        // Método principal para desenhar o conteúdo no inspector
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            // Verifica se esta propriedade está em um subpainel
            var subPanelAttribute = (SubPanelAttribute)attribute;
            bool isSubPanel = subPanelAttribute != null;

            if (isSubPanel && PropertyDrawerExtensions.isInSubPanel && PropertyDrawerExtensions.currentCaption != subPanelAttribute.caption)
            {
                PropertyDrawerExtensions.isInSubPanel = false;

            }

            Rect titleRect = new Rect(position.x, position.y, position.width, 16f);
            // Se estiver no início de um subpainel, desenha o título
            if (isSubPanel && !PropertyDrawerExtensions.isInSubPanel)
            {

                PropertyDrawerExtensions.currentCaption = subPanelAttribute.caption;
                Color originalColor = GUI.color;
                // Define o título do subpainel (label)

                // Ajuste o espaço para a label do subpainel
                EditorGUI.DrawRect(titleRect, subPanelAttribute.backgroundColor);

                // Aqui está a label personalizada que será desenhada antes da propriedade
                GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel); // Cria uma nova GUIStyle para negrito
                boldLabelStyle.fontSize = 14; // Opcional: Defina o tamanho da fonte, caso queira personalizar

                GUI.color = subPanelAttribute.color;
                GUI.Label(titleRect, PropertyDrawerExtensions.currentCaption, boldLabelStyle); // Aplica a label com o estilo
                GUI.color = originalColor;
                position.y += 5f;

                // Atualiza a posição para desenhar a propriedade abaixo da label


                PropertyDrawerExtensions.isInSubPanel = true;  // Marca que estamos dentro de um subpainel
            }




            position.y += 16f;
            EditorGUI.PropertyField(position, property, label);

            if (isSubPanel && PropertyDrawerExtensions.isInSubPanel && PropertyDrawerExtensions.currentCaption != subPanelAttribute.caption)
            {
                position.y += 160f;
                PropertyDrawerExtensions.isInSubPanel = false;
                GUI.Label(titleRect, "CARALHO"); // Aplica a label com o estilo

            }




        }


        private bool IsPrimitive(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.String:
                case SerializedPropertyType.Enum:
                    return true;  // Tipos primitivos conhecidos
                default:
                    return false; // Outros tipos são considerados não primitivos (referências de objetos, structs, etc.)
            }
        }

    }

    public static class PropertyDrawerExtensions
    {



        public static bool isInSubPanel = false;
        public static string currentCaption = "";
        public static float spaceAfterSubPanelTitle = 5f;


        /// <summary>
        /// Gets the parent property of a SerializedProperty
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static SerializedProperty GetParent(this SerializedProperty property)
        {
            var segments = property.propertyPath.Split(new char[] { '.' });
            SerializedProperty matchedProperty = property.serializedObject.FindProperty(segments[0]);
            for (int i = 1; i < segments.Length - 1 && null != matchedProperty; i++)
            {
                matchedProperty = matchedProperty.FindPropertyRelative(segments[i]);
            }

            return matchedProperty;
        }

        public static T GetAttribute<T>(this SerializedProperty property) where T : PropertyAttribute
        {
            var fieldInfo = property.serializedObject.targetObject.GetType().GetField(property.name);
            return fieldInfo?.GetCustomAttribute<T>();
        }
    }


}

#endif