#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Twinny.System.Network
{
    [InitializeOnLoad]
    public static class NetworkInstaller
    {
        static NetworkInstaller()
        {
#if !FUSION2
if (!EditorPrefs.GetBool("Twinny_Fusion2_InstallationStarted", false))
    {
            Debug.LogWarning("[Twinny] Photon Fusion não encontrado! Instalando...");
            EditorPrefs.SetBool("Twinny_Fusion2_InstallationStarted", true);
            string path = "Packages/com.twinny.twe25/Samples~/PF2.unitypackage";
            AssetDatabase.ImportPackage(path, true);
    }
#else
            EditorPrefs.DeleteKey("Twinny_Fusion2_InstallationStarted");
#endif

#if FUSION2 && !PHOTON_VOICE_DEFINED
if (!EditorPrefs.GetBool("Twinny_Voice2_InstallationStarted", false))
    {
            Debug.LogWarning("[Twinny] Photon Voice2 não encontrado! Instalando...");
            EditorPrefs.SetBool("Twinny_Voice2_InstallationStarted", true);

            string path = "Packages/com.twinny.twe25/Samples~/PV2.unitypackage";
            AssetDatabase.ImportPackage(path, true);
}
#elif PHOTON_VOICE_DEFINED
            EditorPrefs.DeleteKey("Twinny_Voice2_InstallationStarted");
#endif
        }
    }
}
#endif