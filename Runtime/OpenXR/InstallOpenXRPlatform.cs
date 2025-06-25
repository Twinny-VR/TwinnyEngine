#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fusion;
using NUnit.Framework;
using Twinny.System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Twinny.XR
{
    [InitializeOnLoad]
    public static class InstallOpenXRPlatform
    {
        private const string OPENXR_SYMBOL = "TWINNY_OPENXR";
        private const string FUSION_NETWORK_CONFIG_PATH = "Assets/Photon/Fusion/Resources/NetworkProjectConfig.fusion";

        static InstallOpenXRPlatform()
        {
            DefinePlatform();
            CheckForSamples();
            AddAssemblyToWeave(typeof(InstallOpenXRPlatform));
        }

        private static void DefinePlatform()
        {
            NamedBuildTarget buildTarget = NamedBuildTarget.Android;
            string defines = PlayerSettings.GetScriptingDefineSymbols(buildTarget);

            if (!defines.Contains(OPENXR_SYMBOL))
            {
                Debug.LogWarning($"[Twinny] Define symbol '{OPENXR_SYMBOL}' added to Android build target.");

                if (string.IsNullOrEmpty(defines))
                    defines = OPENXR_SYMBOL;
                else
                    defines += ";" + OPENXR_SYMBOL;

                PlayerSettings.SetScriptingDefineSymbols(buildTarget, defines);
            }
        }

        public static void AddAssemblyToWeave(Type type)
        {

            var assembly = type.Assembly.GetName().Name;
            var fusionConfig = AssetDatabase.LoadAssetAtPath<NetworkProjectConfigAsset>(FUSION_NETWORK_CONFIG_PATH);
            if (fusionConfig == null)
            {
                Debug.LogError($"[InstallOpenXRPlatform] NetworkProjectConfig not found in '{FUSION_NETWORK_CONFIG_PATH}'");
                return;
            }

            var config = fusionConfig.Config;
            List<string> assemblies = new List<string>(config.AssembliesToWeave);

            if (assemblies.Contains(assembly)) 
                return;

                assemblies.Add(assembly);
                config.AssembliesToWeave = assemblies.ToArray();
                EditorUtility.SetDirty(fusionConfig);
                AssetDatabase.SaveAssets();
                Debug.Log($"Assembly '{assembly}' added to Fusion AssembliesToWeave Network Config File.");
        }


        private static void CheckForSamples()
        {

            if (!Directory.Exists(TwinnyManager.SAMPLE_ROOT))
                return;

            // Procura recursivamente por sample_installed.flag em todas as subpastas
            string[] files = Directory.GetFiles(TwinnyManager.SAMPLE_ROOT, "openxr_sample.cs", SearchOption.AllDirectories);

            if (files.Length > 0)
            {

                bool wantsToConfigure = EditorUtility.DisplayDialog(
            "Configurar cenas na Build Settings?",
            "Deseja configurar as Cenas de Exemplo na lista de Build Scenes agora?",
            "Sim",
            "Não"
        );

                if (wantsToConfigure)
                    ConfigureScenesInBuild(Path.Combine(Path.GetDirectoryName(files[0]), "Example Scenes"));
                foreach (var file in files)
                {
                    AssetDatabase.DeleteAsset(file);
                }
            }
        }
        private static void ConfigureScenesInBuild(string scenePath)
        {
            var scenes = new[]
       {
            "Packages/com.twinny.twe25/Runtime/OpenXR/Scenes/OpenXR_PlatformScene.unity",
            Path.Combine(scenePath,"OpenXR_StartScene.unity"),
            Path.Combine(scenePath,"OpenXR_MockupScene.unity"),
            Path.Combine(scenePath,"OpenXR_HallScene.unity")
        };

            List<EditorBuildSettingsScene> sceneList = new List<EditorBuildSettingsScene>();

            for (int i = 0; i < scenes.Length; i++)
            {
                if (!File.Exists(scenes[i]))
                {
                    Debug.LogWarning($"Scene not found: {scenes[i]}");
                    continue;
                }
                sceneList.Add(new EditorBuildSettingsScene(scenes[i],true));
            }

            EditorBuildSettings.scenes = sceneList.ToArray();

            Debug.Log("Build scenes config successfully!");

        }
    }
}

#endif