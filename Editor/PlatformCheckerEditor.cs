#if UNITY_EDITOR

using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Twinny.Editor
{


[CustomEditor(typeof(MonoBehaviour), true)]
public class PlatformCheckerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
       // _ = CheckOculusProviderStatus();

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

/*
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
*/


    public static void AddDefineSymbol(string symbol)
    {
            BuildTarget currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;

 //       BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(currentBuildTarget); 
//        string existingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            NamedBuildTarget namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            string existingSymbols = PlayerSettings.GetScriptingDefineSymbols(namedTarget);

            var symbols = existingSymbols.Split(';');
            bool hasSymbol = symbols.Select(s => s.Trim()).Any(s => s.Equals(symbol.Trim(), StringComparison.Ordinal));
            Debug.LogWarning(hasSymbol);
        // Adiciona o novo símbolo de definição, se ainda não estiver presente
        if (!hasSymbol)
        {
            string newSymbols = existingSymbols + ";" + symbol;
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, newSymbols);
            Debug.Log($"Script define symbol '{symbol}' adicionado.");
        }
    }

    // Método para remover um símbolo de definição
    public static void RemoveDefineSymbol(string symbol)
    {
            //        BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;  // Você pode mudar isso para iOS, Android, etc.
            //        string existingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            NamedBuildTarget namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            string existingSymbols = PlayerSettings.GetScriptingDefineSymbols(namedTarget);


            // Verifica se o símbolo de definição existe e o remove
            if (existingSymbols.Contains(symbol))
        {
            string newSymbols = existingSymbols.Replace(";" + symbol, "").Replace(symbol + ";", "").Replace(symbol, "");
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, newSymbols);
            Debug.Log($"Script define symbol '{symbol}' removido.");
        }
    }



}

}

#endif