using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Twinny.XR;
using Unity.XR.Oculus;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;


[CustomEditor(typeof(MonoBehaviour), true)]
public class PlatformCheckerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CheckOculusProviderStatus();
    }


    private void CheckXRPlugInStatus()
    {
        bool init = XRGeneralSettings.Instance.InitManagerOnStart;

      
    }


    void CheckOculusProviderStatus()
    {
        // Verifique as configurações gerais do XR
        XRGeneralSettings xrGeneralSettings = XRGeneralSettings.Instance;

        if (xrGeneralSettings != null && xrGeneralSettings.Manager != null)
        {
            var activeLoaders = xrGeneralSettings.Manager.activeLoaders;
            var oculusLoader = activeLoaders.FirstOrDefault(loader => loader is OculusLoader);

            if (oculusLoader != null) { 
                
                AddDefineSymbol("OCULUS");

                if (AssetDatabase.IsValidFolder("Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                string fileName = "RuntimeXRPreset.asset";
                string assetPath = "Assets/Resources/" + fileName;
                RuntimeXR preset = AssetDatabase.LoadAssetAtPath<RuntimeXR>(assetPath);

                if (preset == null) {
                    preset = ScriptableObject.CreateInstance<RuntimeXR>();
                    AssetDatabase.CreateAsset(preset, assetPath);
                    AssetDatabase.SaveAssets();
                    Debug.Log("Novo preset RuntimeXR criado e salvo em: " + assetPath);
                }

            }
            else
            { RemoveDefineSymbol("OCULUS"); }

        }
        else
        {
            Debug.LogError("XRGeneralSettings ou XRManager não estão configurados corretamente.");
        }
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
