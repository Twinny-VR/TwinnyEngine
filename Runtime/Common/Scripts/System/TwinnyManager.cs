using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twinny.Helpers;
using Twinny.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

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

                
                if (XRGeneralSettings.Instance && XRGeneralSettings.Instance.InitManagerOnStart)
                {

                    Platform = Platform.XR;
                    Debug.LogWarning("[TwinnyManager] XR Platform initialized.");
                }
                else
                {

                    Platform = Platform.MOBILE;
                    Debug.LogWarning("[TwinnyManager] Android Platform initialized.");
                }



            }
            else
            if (EditorUserBuildSettings.activeBuildTarget  == BuildTarget.iOS)
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

                if (XRGeneralSettings.Instance && XRGeneralSettings.Instance.InitManagerOnStart)
                {

                    Platform = Platform.XR;
                    Debug.LogWarning("[TwinnyManager] XR Platform initialized.");
                }
                else
                {

                    Platform = Platform.MOBILE;
                    Debug.LogWarning("[TwinnyManager] Android Platform initialized.");
                }



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

            OnPlatformInitialize?.Invoke(Platform);
            CallBackUI.CallAction<IUICallBacks>(callback => callback.OnPlatformInitialize());


        }
    }
}
