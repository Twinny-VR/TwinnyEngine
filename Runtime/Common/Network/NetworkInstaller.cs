#if UNITY_EDITOR
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Twinny.System.Network
{
    [InitializeOnLoad]
    public static class NetworkInstaller
    {
        static NetworkInstaller()
        {
            InstallNetworkPlatform();
        }

        private static async void InstallNetworkPlatform()
        {
            bool hasXRPlugin = await HasPackageAsync("com.unity.xr.management");
            bool hasOpenXR = await HasPackageAsync("com.unity.xr.openxr");

            if (hasXRPlugin && hasOpenXR && EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {

#if !FUSION2
if (!EditorPrefs.GetBool("Twinny_Fusion2_InstallationStarted", false))
    {
            Debug.LogWarning("[Twinny] Photon Fusion n�o encontrado! Instalando...");
           // EditorPrefs.SetBool("Twinny_Fusion2_InstallationStarted", true);
            string path = "Packages/com.twinny.twe25/Samples~/PF2.unitypackage";
         //   AssetDatabase.ImportPackage(path, false);
    }
#else
                EditorPrefs.DeleteKey("Twinny_Fusion2_InstallationStarted");
#endif

#if FUSION2 && !PHOTON_VOICE_DEFINED
if (!EditorPrefs.GetBool("Twinny_Voice2_InstallationStarted", false))
    {
            Debug.LogWarning("[Twinny] Photon Voice2 n�o encontrado! Instalando...");
          //  EditorPrefs.SetBool("Twinny_Voice2_InstallationStarted", true);

            string path = "Packages/com.twinny.twe25/Samples~/PV2.unitypackage";
          //  AssetDatabase.ImportPackage(path, false);
}
#elif PHOTON_VOICE_DEFINED
                EditorPrefs.DeleteKey("Twinny_Voice2_InstallationStarted");
#endif
            }

        }

        public static async Task<bool> HasPackageAsync(string name)
        {
            ListRequest listRequest = Client.List();

            while (!listRequest.IsCompleted)
                await Task.Delay(100);

            if (listRequest.Status == StatusCode.Failure)
            {
                Debug.LogError($"[NetworkInstaller] Falha ao listar pacotes: {listRequest.Error.message}");
                return false;
            }

            foreach (var package in listRequest.Result)
            {
                if (package.name == name)
                {
                    Debug.Log($"[NetworkInstaller] Package '{name}' encontrado!");
                    return true;
                }
            }

            Debug.Log($"[NetworkInstaller] Package '{name}' n�o encontrado!");
            return false;
        }

    }
}
#endif