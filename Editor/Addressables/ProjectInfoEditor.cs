#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.IO;
using System.Linq;
using Twinny.Addressables;

[CustomEditor(typeof(ProjectInfo))]
public class ProjectInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ProjectInfo projectInfo = (ProjectInfo)target;

        if (GUILayout.Button("Setup Addressables"))
        {
            SetupProjectInfoAddressables(projectInfo);
        }
    }

    public static void SetupProjectInfoAddressables(ProjectInfo projectInfo)
    {
        string assetPath = AssetDatabase.GetAssetPath(projectInfo);
        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogError("Não foi possível determinar o path do asset.");
            return;
        }

        string folderPath = Path.GetDirectoryName(assetPath);
        string baseKey = Path.GetFileNameWithoutExtension(assetPath);

        // Pega ou cria AddressablesSettings
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("AddressablesSettings não encontrado. Crie um Addressables Group.");
            return;
        }

        // Pega ou cria o grupo com o mesmo nome do asset
        AddressableAssetGroup group = settings.groups.FirstOrDefault(g => g.Name == projectInfo.name);
        if (group == null)
        {
            group = settings.CreateGroup(projectInfo.name, false, false, false, null, typeof(BundledAssetGroupSchema));
            Debug.Log($"Criado grupo Addressable: {projectInfo.name}");
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true);
        }

        // Torna o ScriptableObject addressable (entry do scriptable)
        string scriptableGuid = AssetDatabase.AssetPathToGUID(assetPath);
        AddressableAssetEntry scriptEntry = settings.FindAssetEntry(scriptableGuid);
        if (scriptEntry == null)
        {
            scriptEntry = settings.CreateOrMoveEntry(scriptableGuid, group);
        }
        // address do scriptable: baseKey
        scriptEntry.address = baseKey;
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, scriptEntry, true);

        // -------------------
        // Thumbnail (procura qualquer texture chamada "thumbnail" na mesma pasta)
        // -------------------
        string[] thumbGuids = AssetDatabase.FindAssets("thumbnail t:Texture2D", new[] { folderPath });
        if (thumbGuids.Length > 0)
        {
            string thumbPath = AssetDatabase.GUIDToAssetPath(thumbGuids[0]);
            string thumbGuid = AssetDatabase.AssetPathToGUID(thumbPath);
            var thumbEntry = settings.FindAssetEntry(thumbGuid) ?? settings.CreateOrMoveEntry(thumbGuid, group);
            thumbEntry.address = $"{baseKey}/thumbnail";
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, thumbEntry, true);
            projectInfo.thumbnailRef = thumbEntry.address;
            Debug.Log($"Thumbnail configurado: {thumbEntry.address}");
        }

        // -------------------
        // Video (procura "video" na mesma pasta)
        // -------------------
        string[] videoGuids = AssetDatabase.FindAssets("video t:VideoClip", new[] { folderPath });
        if (videoGuids.Length > 0)
        {
            string videoPath = AssetDatabase.GUIDToAssetPath(videoGuids[0]);
            string videoGuid = AssetDatabase.AssetPathToGUID(videoPath);
            var videoEntry = settings.FindAssetEntry(videoGuid) ?? settings.CreateOrMoveEntry(videoGuid, group);
            videoEntry.address = $"{baseKey}/video";
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, videoEntry, true);
            projectInfo.videoRef = videoEntry.address;
            Debug.Log($"Video configurado: {videoEntry.address}");
        }

        // -------------------
        // Galery folder
        // -------------------
        string galeryFolder = Path.Combine(folderPath, "galery");
        if (!AssetDatabase.IsValidFolder(galeryFolder))
        {
            AssetDatabase.CreateFolder(folderPath, "galery");
            Debug.Log("Pasta 'galery' criada.");
        }

        // Lista todos os Texture2D dentro da galery
        string[] galeryGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { galeryFolder });
        projectInfo.galeryRef = new string[galeryGuids.Length];

        for (int i = 0; i < galeryGuids.Length; i++)
        {
            string guid = galeryGuids[i];
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var texEntry = settings.FindAssetEntry(guid) ?? settings.CreateOrMoveEntry(guid, group);

            string fileName = Path.GetFileName(path);
            texEntry.address = $"{baseKey}/galery/{fileName}";
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, texEntry, true);

            projectInfo.galeryRef[i] = texEntry.address;
            Debug.Log($"Galery texture configurada: {texEntry.address}");
        }

        // Salva alterações
        EditorUtility.SetDirty(projectInfo);
        AssetDatabase.SaveAssets();
        Debug.Log("Configuração Addressables concluída!");
    }
}
#endif
