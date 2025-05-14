#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Twinny.UI
{
    public static class GameViewResolutionSetter
    {
        static object gameViewSizesInstance;
        static MethodInfo getGroup;

        // Inicializando a reflex�o para acessar os m�todos internos da GameView
        static GameViewResolutionSetter()
        {
            var sizesType = typeof(UnityEditor. Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instanceProp = singleType.GetProperty("instance");
            getGroup = sizesType.GetMethod("GetGroup");
            gameViewSizesInstance = instanceProp.GetValue(null, null);
        }

        // Enum para definir tipos de resolu��o (AspectRatio ou FixedResolution)
        public enum GameViewSizeType
        {
            AspectRatio, FixedResolution
        }

        // Adiciona uma resolu��o personalizada
        public static void AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
        {
            var group = GetGroup(sizeGroupType);
            var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");
            var gvsType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSize");
            var gameViewSizeType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
            var ctor = gvsType.GetConstructor(new Type[] { gameViewSizeType, typeof(int), typeof(int), typeof(string) });

            var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });
            addCustomSize.Invoke(group, new object[] { newSize });
        }

        // M�todo para obter o grupo de resolu��es com base no tipo do grupo
        static object GetGroup(GameViewSizeGroupType type)
        {
            return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
        }

        // M�todo para encontrar a resolu��o pelo nome
        public static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
        {
            var group = GetGroup(sizeGroupType);
            var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
            var displayTexts = getDisplayTexts.Invoke(group, null) as string[];

            for (int i = 0; i < displayTexts.Length; i++)
            {
                string display = displayTexts[i];
                int pren = display.IndexOf('(');
                if (pren != -1)
                    display = display.Substring(0, pren - 1); // Limita a string ao nome da resolu��o

                if (display == text)
                    return i;
            }
            return -1;
        }

        // M�todo para definir uma resolu��o pela string (nome)
        public static void SetGameViewResolution(string label)
        {
            int idx = FindSize(GameViewSizeGroupType.Standalone, label);
            if (idx != -1)
            {
                SetSize(idx);
            }
            else
            {
                Debug.LogWarning($"Resolu��o '{label}' n�o encontrada.");
            }
        }

        // M�todo para definir a resolu��o atrav�s do �ndice
        public static void SetSize(int index)
        {
            var gvWndType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var gvWnd = EditorWindow.GetWindow(gvWndType);
            selectedSizeIndexProp.SetValue(gvWnd, index, null);
        }

        // Fun��o para adicionar e selecionar uma resolu��o customizada
        public static void AddAndSelectCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
        {
            AddCustomSize(viewSizeType, sizeGroupType, width, height, text);
            int idx = FindSize(GameViewSizeGroupType.Standalone, text);
            if (idx != -1)
                SetSize(idx);
            else
                Debug.LogError($"Falha ao encontrar a resolu��o personalizada: {text}");
        }
    }
}
#endif