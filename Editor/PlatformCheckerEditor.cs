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

        // Faz a requisi��o para obter a lista de pacotes instalados
        var request = UnityEditor.PackageManager.Client.List();

        // Aguarda a requisi��o ser conclu�da
        while (!request.IsCompleted)
        {
            await Task.Yield();

        }

        // Verifica se o pacote Plugin Management est� na lista
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
        // Verifica qual plataforma (target) est� sendo usada
        BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;  // Voc� pode mudar isso para iOS, Android, etc.

        // Obt�m os s�mbolos de defini��o existentes para o grupo de constru��o (por exemplo, Standalone)
        string existingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        // Adiciona o novo s�mbolo de defini��o, se ainda n�o estiver presente
        if (!existingSymbols.Contains(symbol))
        {
            string newSymbols = existingSymbols + ";" + symbol;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newSymbols);
            Debug.Log($"Script define symbol '{symbol}' adicionado.");
        }
    }

    // M�todo para remover um s�mbolo de defini��o
    public static void RemoveDefineSymbol(string symbol)
    {
        // Verifica qual plataforma (target) est� sendo usada
        BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;  // Voc� pode mudar isso para iOS, Android, etc.

        // Obt�m os s�mbolos de defini��o existentes
        string existingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        // Verifica se o s�mbolo de defini��o existe e o remove
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