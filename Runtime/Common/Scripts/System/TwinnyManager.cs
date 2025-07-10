using System.IO;
using System.Threading.Tasks;
using Concept.Core;
using Concept.Helpers;
using Twinny.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
#if TWINNY_OPENXR
using UnityEngine.XR.Management;
#endif
namespace Twinny.System
{
    public enum Platform
    {
        UNKNOW,
        WINDOWS,
        XR,
        MOBILE,
        WEBGL
    }
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class TwinnyManager
    {
        public const string PACKAGE_NAME = "com.twinny.twe25";
        const string DEFAULT_KEYSTORE = "TwinnyKey.keystore";
        public const string SAMPLE_ROOT = "Assets/Samples/Twinny Engine";

        public static Platform Platform = Platform.UNKNOW;

        public static TwinnyRuntime config;

        public delegate void onPlatformInitilize(Platform platform);
        public static onPlatformInitilize OnPlatformInitialize;
       
        static TwinnyManager()
        {
#if UNITY_EDITOR && UNITY_ANDROID
            string defaultKeyStore = AssetIO.GetPackageAbsolutePath(PACKAGE_NAME);
            string currentKeyStore = PlayerSettings.Android.keystoreName;
            string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, currentKeyStore);

            bool overwrite = (string.IsNullOrEmpty(currentKeyStore) || !File.Exists(path));

            if(overwrite)
            {

                string newKey = Path.Combine(defaultKeyStore, "Samples~", DEFAULT_KEYSTORE); ;
                Debug.LogWarning($"[TwinnyManager] No valid keystore defined. Using default: '{DEFAULT_KEYSTORE}'.");
                PlayerSettings.Android.keystoreName = newKey;
                EditorUtility.SetDirty(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
                AssetDatabase.SaveAssets();
            }
#endif
        }


        public static async Task InitializePlatform()
        {
            
            //Load current platform StartScene
            AsyncOperation loadScene = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);

            await AsyncOperationExtensions.WaitForSceneLoadAsync(loadScene);


#if UNITY_EDITOR

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {

#if TWINNY_OPENXR
                Platform = Platform.XR;
                Debug.LogWarning("[TwinnyManager] XR Platform initialized.");
#else
                    Platform = Platform.MOBILE;
                    Debug.LogWarning("[TwinnyManager] Android Platform initialized.");
#endif
            }
            else
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                Platform = Platform.MOBILE;
                Debug.LogWarning("[TwinnyManager] iOS Platform initialized.");
            }
            else
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
            {
                Platform = Platform.WINDOWS;
                Debug.LogWarning("[TwinnyManager] Windows Platform initialized.");
            }
            else
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                Platform = Platform.WEBGL;
                Debug.LogWarning("[TwinnyManager] WebGL Platform initialized.");
            }
            else
            {
                Platform = Platform.UNKNOW;
                Debug.LogError($"[TwinnyManager] Unknow Platform initialized ({Application.platform}).");
            }


#else
                if (Application.platform == RuntimePlatform.Android)
            {

#if TWINNY_OPENXR
                Platform = Platform.XR;
                Debug.LogWarning("[TwinnyManager] XR Platform initialized.");
#else
                    Platform = Platform.MOBILE;
                    Debug.LogWarning("[TwinnyManager] Android Platform initialized.");
#endif



            }
            else
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Platform = Platform.MOBILE;
                Debug.LogWarning("[TwinnyManager] iOS Platform initialized.");
            }
            else
            if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
            {
                Debug.LogWarning("[TwinnyManager] nacOS Platform initialized.");
            }
            else
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                Platform = Platform.WINDOWS;
                Debug.LogWarning("[TwinnyManager] Windows Platform initialized.");
            }
            else
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Platform = Platform.WEBGL;
                Debug.LogWarning("[TwinnyManager] WebGL Platform initialized.");
            }
            else
            {
                Platform = Platform.UNKNOW;
                Debug.LogError($"[TwinnyManager] Unknow Platform initialized ({Application.platform}).");
            }
#endif
            if (config.isTestBuild)
            {
                Debug.LogWarning("*** TEST VERSION BUILD! Unset in Config file before Release!");
                if (DebugPanel.Instance != null)
                {
                    DebugPanel.Instance.visible = config.isTestBuild;
                    DebugPanel.Debug("===============================\n" +
                                     "=          <color=#3cbcd6>TEST VERSION BUILD!</color>          =\n" +
                                     "= <color=#3cbcd6>Unset in Config file before Release.</color> =\n" +
                                     "==============================="," ",LogType.Warning);
                }
            }
            CallbackHub.CallAction<IUICallBacks>(callback => callback.OnPlatformInitialize());
            OnPlatformInitialize?.Invoke(Platform);

            if(config != null && config.forceFrameRate)
            {
                Application.targetFrameRate = config.targetFrameRate;
                Debug.LogWarning($"[TwinnyManager] Application frame rate locked at {config.targetFrameRate}FPS.");
            }
        }

        public static void LoadRuntimeProfile<T>(string fileName) where T : TwinnyRuntime
        {
            config = Resources.Load<T>(fileName);

            if (config == null)
            {
                Debug.LogError($"[TwinnyManager] Impossible to load '{fileName}'.");
            }
        }



    }
}
