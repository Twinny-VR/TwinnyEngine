#if UNITY_EDITOR
using Twinny.UI;
using UnityEditor;
using UnityEngine;

namespace Twinny.Editor
{


    [CustomPropertyDrawer(typeof(InfoAttribute))]
    public class InfoAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label); // Ajusta a altura para o texto
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var infoAttribute = (InfoAttribute)attribute;

            // Ajusta a altura da posição para o campo
            position.height = EditorGUI.GetPropertyHeight(property, label, true);

            // Calcula a largura do texto da label
            Vector2 labelSize = GUI.skin.label.CalcSize(label); // Calcula a largura do texto da label

            // Exibe a propriedade normalmente (campo e rótulo)
            EditorGUI.PropertyField(position, property, label);

            // Calcula a largura do campo, subtraindo a largura da label
            float fieldWidth = position.width - labelSize.x;

            // Ajusta a posição do texto com base na largura da label
            Rect infoRect = new Rect(position.x + labelSize.x, position.y, fieldWidth, 20f);
           
            GUIContent content = new GUIContent($"<i><size=10>({infoAttribute.message})</size></i>");

            GUIStyle style = new GUIStyle(EditorStyles.label)
            {
                richText = true
            };

            // Exibe o texto do Info ao lado da propriedade
            EditorGUI.LabelField(infoRect, content, style);
        }


        /*
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var infoAttribute = (InfoAttribute)attribute;
            position.height = EditorGUI.GetPropertyHeight(property, label, true);

            // Exibe a variável normalmente
            EditorGUI.PropertyField(position, property, label);
            Vector2 labelSize = GUI.skin.label.CalcSize(label);
            
            GUI.skin.label.richText = true;

            label.text += "CUZINHO";
                
            float labelWidth = position.width - labelSize.x;

            // Adiciona o texto do Info abaixo da propriedade
            Rect infoRect = new Rect(labelWidth, position.y, position.width, 20f);
            EditorGUI.LabelField(infoRect, infoAttribute.message, EditorStyles.helpBox);
        }*/
    }

}

#endif