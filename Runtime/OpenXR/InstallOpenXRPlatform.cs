#if UNITY_EDITOR
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
    }
}

#endif