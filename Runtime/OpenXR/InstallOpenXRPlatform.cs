#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
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
        private const string Symbol = "TWINNY_OPENXR";

        static InstallOpenXRPlatform()
        {
            DefinePlatform();
            CheckForSamples();
        }

        private static void DefinePlatform()
        {
            NamedBuildTarget buildTarget = NamedBuildTarget.Android;
            string defines = PlayerSettings.GetScriptingDefineSymbols(buildTarget);

            if (!defines.Contains(Symbol))
            {
                Debug.LogWarning($"[Twinny] Define symbol '{Symbol}' added to Android build target.");

                if (string.IsNullOrEmpty(defines))
                    defines = Symbol;
                else
                    defines += ";" + Symbol;

                PlayerSettings.SetScriptingDefineSymbols(buildTarget, defines);
            }
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