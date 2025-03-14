#if UNITY_EDITOR

using System;
using System.Threading.Tasks;
using Twinny.XR;
using UnityEditor;
using UnityEngine;

namespace Twinny.Editor
{


[CustomEditor(typeof(MonoBehaviour), true)]
public class PlatformCheckerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        _ = CheckOculusProviderStatus();

    }

    async Task<bool> CheckPluginManagement(string packageName)
    {
        bool isPluginManagementInstalled = false;

        // Faz a requisição para obter a lista de pacotes instalados
        var request = UnityEditor.PackageManager.Client.List();

        // Aguarda a requisição ser concluída
        while (!request.IsCompleted)
        {
            await Task.Yield();

        }

        // Verifica se o pacote Plugin Management está na lista
        if (request.Status == UnityEditor.PackageManager.StatusCode.Success)
        {
            foreach (var package in request.Result)
            {
                if (package.name.Contains(packageName))
                {
                    isPluginManagementInstalled = true;
                    break;
                }
            }
        }
        return isPluginManagementInstalled;
    }

    async Task CheckOculusProviderStatus()
    {

            BuildTarget currentPlatform = EditorUserBuildSettings.activeBuildTarget;

            bool hasOculus = await CheckPluginManagement("com.meta.xr.sdk.all");
        if (currentPlatform == BuildTarget.Android && hasOculus)
        {
            AddDefineSymbol("OCULUS");

            if (AssetDatabase.IsValidFolder("Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string fileName = "RuntimeXRPreset.asset";
            string assetPath = "Assets/Resources/" + fileName;
            RuntimeXR preset = AssetDatabase.LoadAssetAtPath<RuntimeXR>(assetPath);

            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<RuntimeXR>();
                AssetDatabase.CreateAsset(preset, assetPath);
                AssetDatabase.SaveAssets();
                Debug.Log("Novo preset RuntimeXR criado e salvo em: " + assetPath);
            }

        }
        else
            RemoveDefineSymbol("OCULUS");
    }


    public static void AddDefineSymbol(string symbol)
    {
        // Verifica qual plataforma (target) está sendo usada
        BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;  // Você pode mudar isso para iOS, Android, etc.

        // Obtém os símbolos de definição existentes para o grupo de construção (por exemplo, Standalone)
        string existingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        // Adiciona o novo símbolo de definição, se ainda não estiver presente
        if (!existingSymbols.Contains(symbol))
        {
            string newSymbols = existingSymbols + ";" + symbol;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newSymbols);
            Debug.Log($"Script define symbol '{symbol}' adicionado.");
        }
    }

    // Método para remover um símbolo de definição
    public static void RemoveDefineSymbol(string symbol)
    {
        // Verifica qual plataforma (target) está sendo usada
        BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;  // Você pode mudar isso para iOS, Android, etc.

        // Obtém os símbolos de definição existentes
        string existingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        // Verifica se o símbolo de definição existe e o remove
        if (existingSymbols.Contains(symbol))
        {
            string newSymbols = existingSymbols.Replace(";" + symbol, "").Replace(symbol + ";", "").Replace(symbol, "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newSymbols);
            Debug.Log($"Script define symbol '{symbol}' removido.");
        }
    }



}

}

#endif