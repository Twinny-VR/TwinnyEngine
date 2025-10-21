#if OBSOLETE
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace Twinny.System
{


    [CustomEditor(typeof(DynamicMeshSpawner))]
    public class DynamicMeshSpawnerEditor : UnityEditor.Editor
    {
        private GUIStyle _labelStyle;
        private Texture2D _backgroundTexture;
        private DynamicMeshSpawner _target;

        private void OnEnable()
        {
            _target = (DynamicMeshSpawner)target;
        }

        public override void OnInspectorGUI()
        {
            if(_labelStyle == null)
            {
                _backgroundTexture = MakeTex(1, 1, Color.black);
                _labelStyle = new GUIStyle(EditorStyles.label);
                _labelStyle.normal.textColor = Color.green;
                _labelStyle.alignment = TextAnchor.MiddleCenter;
                _labelStyle.fontStyle = FontStyle.Bold;
                _labelStyle.normal.background = _backgroundTexture;
                _labelStyle.padding = new RectOffset(4, 4, 4, 4);
            }

            DynamicMeshSpawner spawner = (DynamicMeshSpawner)target;

            // Pega valores
            int delay = spawner.spawnInterval;
            int chunkSize = spawner.chunkSize;
            int listCount = spawner.renderers != null ? spawner.renderers.Count : 0;

            // Calcula o tempo total em ms
            int chunks = Mathf.CeilToInt((float)listCount / Mathf.Max(chunkSize, 1));
            int totalMs = chunks * delay;

            // Formata o tempo pra algo legível
            string timeStr = FormatMilliseconds(totalMs);

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Estimated Spawn Time: ({timeStr})", _labelStyle);
            EditorGUILayout.Space();


            // Botão para popular lista (se quiser manter)
            EditorGUILayout.Space();
            if (GUILayout.Button("Fill Renderer List"))
            {
                Undo.RecordObject(spawner, "Fill m_renderers");

                Renderer[] renderers = spawner.GetComponentsInChildren<Renderer>(true);

                spawner.GetType()
                       .GetField("renderers", BindingFlags.Public | BindingFlags.Instance)
                       .SetValue(spawner, new List<Renderer>(renderers));

                EditorUtility.SetDirty(spawner);
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("HideAll")) 
                _= _target.SpawnAsync(false, 0);
            if (GUILayout.Button("ShowAll")) 
                _= _target.SpawnAsync(true, 0);
            EditorGUILayout.EndHorizontal();

        }

        // Método utilitário para criar textura 1x1 colorida
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        // Formata milissegundos pra string legível (ex: 3s 250ms)
        private string FormatMilliseconds(int ms)
        {
            int totalSeconds = ms / 1000;

            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            string stamp = seconds + "s"; 
            if(minutes > 0) stamp = $"{minutes:D2}:{seconds:D2}";
            if (hours > 0) stamp = $"{hours}:{stamp}";
            return stamp;
        }


    }

}


#endif