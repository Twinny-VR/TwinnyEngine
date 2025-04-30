using Twinny.Helpers;
using Twinny.UI;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace Twinny.System.Cameras
{
    public class CameraManager : TSingleton<CameraManager>
    {

        [SerializeField] private FirstPersonAgent _fpsAgent;

        [DrawScriptable]
        [SerializeField] private CameraRuntime _config;
        public static CameraRuntime config { get => Instance?._config; }

        #region MonoBehaviour Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
            string resPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.Refresh();
            }

            string fileName = "CameraRuntimePreset.asset";
            string assetPath = resPath + "/" + fileName;


            CameraRuntime preset = AssetDatabase.LoadAssetAtPath<CameraRuntime>(assetPath);

            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<CameraRuntime>();
                AssetDatabase.CreateAsset(preset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.LogWarning("[CameraHandler] Novo preset 'CameraRuntimePreset' criado e salvo em: " + assetPath);
            }

            _config = AssetDatabase.LoadAssetAtPath<CameraRuntime>(assetPath);

        }

#endif


        protected override void Awake()
        {
            base.Awake();
            _config = Resources.Load<CameraRuntime>("CameraRuntimePreset");

            if (_config == null)
            {
                Debug.LogError("[CameraHandler] Impossible to load 'CameraRuntimePreset'.");
            }

        }

        #endregion

        #region Public Methods

        #endregion

    }
}
