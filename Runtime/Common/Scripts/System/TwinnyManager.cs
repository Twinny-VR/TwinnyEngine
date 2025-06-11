using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Concept.Core;
using Concept.Helpers;
using Twinny.Helpers;
using Twinny.UI;
using UnityEditor;
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

    public static class TwinnyManager
    {
        public static Platform Platform = Platform.UNKNOW;

        public static TwinnyRuntime config;


        public delegate void onPlatformInitilize(Platform platform);
        public static onPlatformInitilize OnPlatformInitialize;
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
