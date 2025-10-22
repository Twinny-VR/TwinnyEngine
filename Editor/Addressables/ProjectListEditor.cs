#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.IO;
using System.Linq;

[CustomEditor(typeof(Twinny.Addressables.ProjectList))]
public class ProjectListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Twinny.Addressables.ProjectList projectList = (Twinny.Addressables.ProjectList)target;

        if (GUILayout.Button("List Projects"))
        {
            ListAllProjects(projectList);
        }
    }

    private void ListAllProjects(Twinny.Addressables.ProjectList projectList)
    {
        string listPath = AssetDatabase.GetAssetPath(projectList);
        if (string.IsNullOrEmpty(listPath))
        {
            Debug.LogError("Não foi possível determinar o path do ProjectList.");
            return;
        }

        string folderPath = Path.GetDirectoryName(listPath);
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("AddressablesSettings não encontrado. Crie um Addressables Group antes de continuar.");
            return;
        }

        // Encontra todos os ProjectInfo.asset dentro da pasta e subpastas
        string[] projectInfoGuids = AssetDatabase.FindAssets("t:ProjectInfo", new[] { folderPath });
        if (projectInfoGuids.Length == 0)
        {
            Debug.LogWarning("Nenhum ProjectInfo encontrado dentro da pasta do ProjectList.");
            return;
        }

        string[] addresses = new string[projectInfoGuids.Length];

        for (int i = 0; i < projectInfoGuids.Length; i++)
        {
            string guid = projectInfoGuids[i];
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string assetName = Path.GetFileNameWithoutExtension(path);

            // Garante que tenha um grupo com o mesmo nome
            AddressableAssetGroup group = settings.groups.FirstOrDefault(g => g.Name == assetName);
            if (group == null)
            {
                group = settings.CreateGroup(assetName, false, false, false, null, typeof(BundledAssetGroupSchema));
                Debug.Log($"Criado grupo Addressable: {assetName}");
            }

            // Torna o ProjectInfo addressable, se ainda não for
            AddressableAssetEntry entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, group);
            }
            else if (entry.parentGroup != group)
            {
                // Move pro grupo correto se estiver em outro
                settings.MoveEntry(entry, group);
            }

            // Define o address com o nome do asset (ex: project_info_platea)
            entry.address = assetName;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entry, true);

            addresses[i] = entry.address;
            Debug.Log($"ProjectInfo configurado: {entry.address}");
        }

        // Atualiza o ProjectList
        projectList.projectGroups = addresses;
        EditorUtility.SetDirty(projectList);
        AssetDatabase.SaveAssets();

        // Adiciona o próprio ProjectList como Addressable
        string listGuid = AssetDatabase.AssetPathToGUID(listPath);
        string listFileName = Path.GetFileNameWithoutExtension(listPath);

        // Cria grupo se não existir
        AddressableAssetGroup listGroup = settings.groups.FirstOrDefault(g => g.Name == listFileName);
        if (listGroup == null)
        {
            listGroup = settings.CreateGroup(listFileName, false, false, false, null, typeof(BundledAssetGroupSchema));
            Debug.Log($"Criado grupo Addressable: {listFileName}");
        }

        // Adiciona ou move o scriptable para o grupo
        AddressableAssetEntry listEntry = settings.FindAssetEntry(listGuid);
        if (listEntry == null)
        {
            listEntry = settings.CreateOrMoveEntry(listGuid, listGroup);
            Debug.Log($"Adicionado ProjectList '{listFileName}' como Addressable.");
        }
        else if (listEntry.parentGroup != listGroup)
        {
            settings.MoveEntry(listEntry, listGroup);
            Debug.Log($"Movido ProjectList '{listFileName}' para o grupo correto.");
        }

        listEntry.address = listFileName;
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, listEntry, true);
        AssetDatabase.SaveAssets();


        Debug.Log($"ProjectList atualizado com {addresses.Length} projetos!");
    }
}
#endif
