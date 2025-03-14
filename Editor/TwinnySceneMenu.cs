using UnityEditor;
using UnityEngine;
using Twinny.System;
using Twinny.XR;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Twinny.Editor
{

    public class TwinnySceneMenu
    {
        public const string ROOT_NAME = "root";
        public const string WORLD_NAME = "world";



#if OCULUS
        [MenuItem("Twinny/Platforms/Set Meta Quest Platform")]
        private static void SetMetaQuest()
        {
            SetPlatformScenes("MetaQuest");
            string oculusFolder = "Assets/Oculus";
            if (!AssetDatabase.IsValidFolder(oculusFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Oculus");
            }
            string originalProjectConfig = $"Packages/com.twinny.twe25/Runtime/Oculus/OculusProjectConfig.asset";
            string destinyPath = Path.Combine(oculusFolder, "OculusProjectConfig.asset");
            //            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Oculus/") == null)
            if (!AssetDatabase.CopyAsset(originalProjectConfig, destinyPath))
            {
                Debug.LogError($"Failed to copy '{originalProjectConfig}' to '{destinyPath}.");
            }
            else
                Debug.LogWarning($"'OculusProjectConfig' created in {oculusFolder}");

        }

        [MenuItem("Twinny/Platforms/Create Meta Quest Platform Preset")]
        public static void CreateMetaPreset()
        {
            CreateRuntimePreset<RuntimeXR>();
        }

                [MenuItem("Twinny/Scenes/New VR Scene")]
        [MenuItem("Assets/Create/Twinny/Scenes/New VR Scene")]
        private static void CreateVRScene()
        {
            CreateScene(SceneType.VR);
        }

        [MenuItem("Twinny/Scenes/New MR Scene")]
        [MenuItem("Assets/Create/Twinny/Scenes/New MR Scene")]
        private static void CreateMRScene()
        {
            CreateScene(SceneType.MR);
        }

#else


        [MenuItem("Twinny/Platforms/Set Windows Platform")]
        private static void SetWinPlatform()
        {
            SetPlatformScenes("Win");
        }


        [MenuItem("Twinny/Platforms/Create Multi-Platform Preset")]
        public static void CreatePreset()
        {
            CreateRuntimePreset<MultiPlatformRuntime>();
        }
#endif
        private static void SetPlatformScenes(string platform)
        {
            var newScenes = new List<EditorBuildSettingsScene>();


            string originalPlatformScene = $"Packages/com.twinny.twe25/Runtime/PlatformScenes/{platform}PlatformScene.unity";
            string originalPlatformStartScene = $"Packages/com.twinny.twe25/Runtime/PlatformScenes/{platform}StartScene.unity";
            string originalPlatformMockupScene = $"Packages/com.twinny.twe25/Runtime/PlatformScenes/{platform}MockupScene.unity";


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
            "Salvar Preset",                  // Título da janela
            preset.GetType().Name + "Preset",                // Nome do arquivo sugerido
            "asset",                          // Extensão do arquivo
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
                Debug.LogWarning("O usuário cancelou a criação do preset.");
            }

        }




        [MenuItem("Twinny/Scenes/New Mobile Scene")]
        [MenuItem("Assets/Create/Twinny/Scenes/New Mobile Scene")]
        private static void CreateMobileScene()
        {
            CreateScene(SceneType.MOBILE);
        }


        private static void CreateScene(SceneType type)
        {


            // Pergunta ao usuário o nome da cena
            string path = EditorUtility.SaveFilePanel(
                "Save New Scene",
                "Assets/Scenes", // Diretório inicial
                $"New{type.ToString()}Scene", // Nome padrão
                "unity" // Extensão do arquivo
            );

            path = GetRelativePath(path);


            // Verifica se o usuário forneceu um nome válido
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // Cria uma nova cena com um nome específico
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Verifica se a cena foi criada corretamente
            if (newScene.IsValid())
            {
                // Chama o método para adicionar objetos predefinidos à cena
                AddPredefinedObjectsToScene(type);

                // Salva a cena com um nome padrão (opcional)
                //string path = $"Assets/Scenes/New{type.ToString()}Scene.unity";
                EditorSceneManager.SaveScene(newScene, path);

                Debug.LogWarning("[TwinnySceneMenu] New scene created and saved at: " + path);

                // Atualiza a janela do Project
                AssetDatabase.Refresh();

                // Seleciona o diretório onde a cena foi salva
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

        // Método para transformar o caminho absoluto em relativo
        private static string GetRelativePath(string absolutePath)
        {
            // Verifica se o caminho é válido
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

        // Método para adicionar objetos predefinidos à cena
        private static void AddPredefinedObjectsToScene(SceneType type)
        {
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


        }


    }
}