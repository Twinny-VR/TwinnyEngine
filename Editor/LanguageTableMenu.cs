
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Twinny.Localization;
using UnityEditor;
using UnityEngine;


namespace Twinny.Editor
{
    public class LanguageTableMenu
    {
        public const string LOCALIZATION_PATH = "Assets/Settings/Language/";

        // MenuItem para criar um novo Language Table
        [MenuItem("Twinny/Localization/New Language Table")]
        private static void CreateNewLanguageFile()
        {
            string path = LOCALIZATION_PATH + "NewLanguageTable.asset";

            // Verifica se a pasta existe, se não, cria
            if (!Directory.Exists(LOCALIZATION_PATH))
            {
                Directory.CreateDirectory(LOCALIZATION_PATH);
                AssetDatabase.Refresh();  // Atualiza o AssetDatabase para incluir a nova pasta
            }

            string referenceAssetPath = "Packages/com.twinny.twe25/Runtime/Scripts/Tables/Languages/PT-BR.asset"; // Caminho do arquivo de referência
            LanguageTable referenceTable = AssetDatabase.LoadAssetAtPath<LanguageTable>(referenceAssetPath);

            if (referenceTable == null)
            {
                Debug.LogError("[LanguageTableMenu] Reference table not found at: " + referenceAssetPath);
                return; // Se o arquivo de referência não existir, o processo é interrompido
            }

            // Cria uma nova instância do LanguageTable
            LanguageTable newLanguageTable = ScriptableObject.CreateInstance<LanguageTable>();

            // Copia os dados do arquivo de referência para o novo asset
            newLanguageTable.stringEntries = referenceTable.stringEntries; // Copiando os entries de strings
                                                                           // Cria e salva o novo asset na pasta especificada
            AssetDatabase.CreateAsset(newLanguageTable, path);

            // Salva o asset no disco
            AssetDatabase.SaveAssets();

            // Seleciona o asset criado na janela Project
            EditorUtility.FocusProjectWindow();

            // Log para confirmar a criação
            Debug.LogWarning("New Language File created in: " + path);

            // Seleciona o diretório onde a cena foi salva
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            Selection.activeObject = asset;

            // Mostra a pasta na aba Project
            EditorGUIUtility.PingObject(asset);
        }
    }

}
#endif