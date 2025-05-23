#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Twinny.System;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using Twinny.Localization;
#if OCULUS
using Twinny.XR;
#endif

namespace Twinny.Editor
{

    public class TwinnySceneMenu
    {
        public const string ROOT_NAME = "root";
        public const string WORLD_NAME = "world";

        [MenuItem("Twinny/Realtime Settings")]
        public static void PingConfigAsset()
        {
            if (TwinnyManager.config != null)
            {
                EditorApplication.ExecuteMenuItem("Window/General/Project");
                Selection.activeObject = TwinnyManager.config;
                EditorGUIUtility.PingObject(TwinnyManager.config);
            }
            else
            {
                Debug.LogWarning("TwinnyRuntime config is null. Asset might not be loaded yet.");
            }
        }


#if !WIN
        [MenuItem("Twinny/Platforms/Windows/Install Windows Platform")]
        private static void InstallWindows()
        {
            InstallPlatform("Win");
        }

#else

    [MenuItem("Twinny/Platforms/Windows/Set Windows Platform")]
        private static void SetWinPlatform()
        {
            SetPlatformScenes("Win");
        }



        [MenuItem("Twinny/Platforms/Windows/Uninstall Windows Platform")]
        private static void UninstallWindows()
        {
            UninstallPlatform("Win");
        }


#endif


#if !MOBILE
        [MenuItem("Twinny/Platforms/Mobile/Install Mobile Platform")]
        private static void InstallMobile()
        {
            InstallPlatform("Mobile");
        }

#else

    [MenuItem("Twinny/Platforms/Mobile/Set Mobile Platform")]
        private static void SetWinPlatform()
        {
            SetPlatformScenes("Mobile");
        }



        [MenuItem("Twinny/Platforms/Mobile/Uninstall Mobile Platform")]
        private static void UninstallMobile()
        {
            UninstallPlatform("Mobile");
        }

        [MenuItem("Twinny/Platforms/Mobile/New Mobile Scene")]
        [MenuItem("Assets/Create/Twinny/Scenes/New Mobile Scene")]
        private static void CreateMobileScene()
        {
            CreateScene(SceneType.MOBILE);
        }



#endif

#if !OCULUS
        [MenuItem("Twinny/Platforms/Meta Quest/Install Meta Quest Platform")]
        private static void InstallMetaQuest()
        {
            InstallPlatform("Oculus");
        }




#else
        [MenuItem("Twinny/Platforms/Set Meta Quest Platform")]
        private static void SetMetaQuest()
        {
            SetPlatformScenes("MetaQuest");
            string oculusFolder = "Assets/Meta/Oculus";
            if (!AssetDatabase.IsValidFolder(oculusFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Meta/Oculus");
            }
            string originalProjectConfig = $"Packages/com.twinny.twe25/Runtime/Meta/Oculus/OculusProjectConfig.asset";
            string destinyPath = Path.Combine(oculusFolder, "OculusProjectConfig.asset");
            //            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Meta/Oculus/") == null)
            if (!AssetDatabase.CopyAsset(originalProjectConfig, destinyPath))
            {
                Debug.LogError($"Failed to copy '{originalProjectConfig}' to '{destinyPath}.");
            }
            else
                Debug.LogWarning($"'OculusProjectConfig' created in {oculusFolder}");

        }

        [MenuItem("Twinny/Platforms/Meta Quest/Create Meta Quest Platform Preset")]
        public static void CreateMetaPreset()
        {
            CreateRuntimePreset<RuntimeXR>();
        }

                [MenuItem("Twinny/Platforms/Meta Quest/Scenes/New VR Scene")]
        [MenuItem("Assets/Create/Twinny/Scenes/New VR Scene")]
        private static void CreateVRScene()
        {
            CreateScene(SceneType.VR);
        }

        [MenuItem("Twinny/Scenes/New MR Scene")]
        [MenuItem("Assets/Create/Twinny/Platforms/Meta Quest/Scenes/New MR Scene")]
        private static void CreateMRScene()
        {
            CreateScene(SceneType.MR);
        }

        [MenuItem("Twinny/Platforms/Meta Quest/Uninstall Meta Quest Platform")]
        private static void UninstallMetaQuest()
        {

            UninstallPlatform("Oculus");



        }


#endif



#if !NETWORK
        [MenuItem("Twinny/Platforms/Network/Install Multiplayer Platform")]
        private static void InstallNetwork()
        {
            InstallPlatform("Network");
        }

#else

        [MenuItem("Twinny/Platforms/Network/Uninstall Multiplayer Platform")]
        private static void UninstallNetwork()
        {
            UninstallPlatform("Network");
        }


#endif


        public static void InstallPlatform(string platform, bool prompt = true)
        {

            bool shouldInstallPlatform = true;

            if (prompt) shouldInstallPlatform = EditorUtility.DisplayDialog(
                string.Format(LocalizationProvider.GetTranslated("%INSTALL_PLATFORM"), LocalizationProvider.GetTranslated(platform)),
                string.Format(LocalizationProvider.GetTranslated("%INSTALL_PLATFORM_MESSAGE"), LocalizationProvider.GetTranslated(platform)),
                LocalizationProvider.GetTranslated("YES"),
                LocalizationProvider.GetTranslated("NO")
            );

            if (shouldInstallPlatform)
            {



                string path = $"Packages/com.twinny.twe25/Runtime/{platform}~";
                string newPath = path.TrimEnd('~');
                if (Directory.Exists(path))
                {

                    try
                    {
                        if (Directory.Exists(newPath) && Directory.GetFiles(newPath).Length == 0 && Directory.GetDirectories(newPath).Length == 0)
                        {
                            Directory.Delete(newPath);
                        }


                        Directory.Move(path, newPath);
                        AssetDatabase.Refresh();


                        if (platform != "Network")
                        {

                            bool shouldSetPlatform = EditorUtility.DisplayDialog(
                               string.Format(LocalizationProvider.GetTranslated("%INSTALL_PLATFORM"), LocalizationProvider.GetTranslated(platform)),
                               LocalizationProvider.GetTranslated("%PLATFORM_INSTALLED"),
                               LocalizationProvider.GetTranslated("YES"),
                               LocalizationProvider.GetTranslated("NO")
                            );
                            
                            if (shouldSetPlatform) SetPlatformScenes(platform);
                        }

                        PlatformCheckerEditor.AddDefineSymbol(platform.ToUpper());

                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to rename orginal path: {e.Message}");
                    }
                }
                else
                if (Directory.Exists(newPath) && (Directory.GetFiles(newPath).Length > 0 || Directory.GetDirectories(newPath).Length > 0))
                {
                        PlatformCheckerEditor.AddDefineSymbol(platform.ToUpper());
                }
                else
                Debug.LogError($"Directory not found: {path}");

#if FUSION2 && !NETWORK
              if(platform != "Network") InstallPlatform("Network");


#endif
            }

        }
        public static void UninstallPlatform(string platform)
        {

            PlatformCheckerEditor.RemoveDefineSymbol(platform.ToUpper());

            string path = $"Packages/com.twinny.twe25/Runtime/{platform}";
            if (Directory.Exists(path))
            {

                try
                {
                    Directory.Move(path, path + '~');
                    AssetDatabase.Refresh();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to rename orginal path: {e.Message}");
                }
            }
            else
                Debug.LogError($"Directory not found: {path}");



        }




#if WIN
        [MenuItem("Twinny/Platforms/Create Multi-Platform Preset")]
        public static void CreatePreset()
        {
            CreateRuntimePreset<MultiPlatformRuntime>();
        }
#endif
        private static void SetPlatformScenes(string platform)
        {
            var newScenes = new List<EditorBuildSettingsScene>();


            string originalPlatformScene = $"Packages/com.twinny.twe25/Runtime/{platform}/Scenes/{platform}PlatformScene.unity";
            string originalPlatformStartScene = $"Packages/com.twinny.twe25/Runtime/{platform}/Scenes/{platform}StartScene.unity";
            string originalPlatformMockupScene = $"Packages/com.twinny.twe25/Runtime/{platform}/Scenes/{platform}MockupScene.unity";


            string scenesFolder = "Assets/Scenes";
            if (!AssetDatabase.IsValidFolder(scenesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            string platformFolder = Path.Combine(scenesFolder, platform);
            if (!AssetDatabase.IsValidFolder(platformFolder))
            {
                AssetDatabase.CreateFolder(scenesFolder, platform);
            }

            newScenes.Add(new EditorBuildSettingsScene(originalPlatformScene, true));

            string destinyPath = Path.Combine(platformFolder, $"{platform}StartScene.unity");
            // Copia a cena de dentro do pacote para o destino


            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destinyPath) == null)
                if (!AssetDatabase.CopyAsset(originalPlatformStartScene, destinyPath))
                {
                    Debug.LogError($"Failed to copy '{originalPlatformStartScene}' to '{destinyPath}.");
                    return;
                }

            newScenes.Add(new EditorBuildSettingsScene(destinyPath, true));

            destinyPath = Path.Combine(platformFolder, $"{platform}MockupScene.unity");
            // Copia a cena de dentro do pacote para o destino


            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(originalPlatformMockupScene) != null)
            {


                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destinyPath) == null)
                    if (!AssetDatabase.CopyAsset(originalPlatformMockupScene, destinyPath))
                    {
                        Debug.LogError($"Failed to copy '{originalPlatformMockupScene}' to '{destinyPath}.");
                        return;
                    }

            }

            newScenes.Add(new EditorBuildSettingsScene(destinyPath, true));


            //  var inBuildScenes = EditorBuildSettings.scenes;


            EditorBuildSettings.scenes = newScenes.ToArray();
            Debug.LogWarning("Scenes In Build setted to Meta Quest Platform.");

        }
        public static void CreateRuntimePreset<T>() where T : TwinnyRuntime
        {
            T preset = ScriptableObject.CreateInstance<T>();


            if (AssetDatabase.IsValidFolder("Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string filePath = EditorUtility.SaveFilePanelInProject(
            "Salvar Preset",                  // T�tulo da janela
            preset.GetType().Name + "Preset",                // Nome do arquivo sugerido
            "asset",                          // Extens�o do arquivo
            "Escolha onde salvar o preset",   // Texto de ajuda
            "Assets/Resources"                        // Caminho inicial sugerido (Assets/Resources)
        );


            if (!string.IsNullOrEmpty(filePath))
            {
                // Cria o asset no caminho escolhido
                AssetDatabase.CreateAsset(preset, filePath);
                AssetDatabase.SaveAssets();

                Debug.Log($"Preset salvo em: {filePath}");

                string relativePath = filePath.Substring(filePath.IndexOf("Assets"));  // Pega o caminho relativo a partir de Assets/
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(relativePath);

                // Seleciona o asset e foca na aba "Project"
                Selection.activeObject = asset;
                EditorUtility.FocusProjectWindow(); // Foca a janela do Project no asset


            }
            else
            {
                Debug.LogWarning("O usu�rio cancelou a cria��o do preset.");
            }

        }






        private static void CreateScene(SceneType type)
        {


            // Pergunta ao usu�rio o nome da cena
            string path = EditorUtility.SaveFilePanel(
                "Save New Scene",
                "Assets/Scenes", // Diret�rio inicial
                $"New{type.ToString()}Scene", // Nome padr�o
                "unity" // Extens�o do arquivo
            );

            path = GetRelativePath(path);


            // Verifica se o usu�rio forneceu um nome v�lido
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // Cria uma nova cena com um nome espec�fico
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Verifica se a cena foi criada corretamente
            if (newScene.IsValid())
            {
                // Chama o m�todo para adicionar objetos predefinidos � cena
                AddPredefinedObjectsToScene(type);

                // Salva a cena com um nome padr�o (opcional)
                //string path = $"Assets/Scenes/New{type.ToString()}Scene.unity";
                EditorSceneManager.SaveScene(newScene, path);

                Debug.LogWarning("[TwinnySceneMenu] New scene created and saved at: " + path);

                // Atualiza a janela do Project
                AssetDatabase.Refresh();

                // Seleciona o diret�rio onde a cena foi salva
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
                Selection.activeObject = asset;

                // Mostra a pasta na aba Project
                EditorGUIUtility.PingObject(asset);

            }
            else
            {
                Debug.LogError($"Failed to create a new {type.ToString()} scene.");
            }
        }

        // M�todo para transformar o caminho absoluto em relativo
        private static string GetRelativePath(string absolutePath)
        {
            // Verifica se o caminho � v�lido
            if (string.IsNullOrEmpty(absolutePath))
                return null;

            // Converte o caminho absoluto para o relativo
            string projectPath = Application.dataPath;  // "C:/Projects/SeuProjeto/Assets"
            if (absolutePath.StartsWith(projectPath))
            {
                // Substitui o caminho do projeto para "Assets" e retorna o caminho relativo
                return "Assets" + absolutePath.Substring(projectPath.Length);
            }
            else
                Debug.LogError("[TwinnySceneMenu] Scenes must be created inside project folder.");
            return null;
        }

        // M�todo para adicionar objetos predefinidos � cena
        private static void AddPredefinedObjectsToScene(SceneType type)
        {
            /*
            GameObject root = new GameObject();
            root.name = ROOT_NAME;
            SceneFeatureXR feature = root.AddComponent<SceneFeatureXR>();
            feature.sceneType = type;
            GameObject world = new GameObject();
            world.name = WORLD_NAME;
            world.transform.parent = root.transform;
            feature.worldTransform = world.transform;


            GameObject safeAreaPrefab = Resources.Load<GameObject>("UI/SAFE_AREA");
            if (safeAreaPrefab)
            {
                GameObject safeArea = (GameObject)PrefabUtility.InstantiatePrefab(safeAreaPrefab);
                safeArea.transform.parent = root.transform;
            }
            else
                Debug.LogWarning("SafeArea prefab not found! Verify if the path 'UI/SAFE_AREA' is correct.");

            */
        }


    }
}
#endif