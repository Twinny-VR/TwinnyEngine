#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Twinny.System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Twinny.Localization;
using System.IO;
using Twinny.XR;
using System;
using UnityEditor.Presets;


namespace Twinny.Helpers
{
    public class LandMarkMenu
    {
        public const string ROOT_NAME = "root";
        public const string WORLD_NAME = "world";
        public const string LAND_MARK_ROOT_NAME = "[Twinny]LandMarks";


        [MenuItem("Twinny/Land Mark NODE %l")]  // O _l significa Ctrl + L (para Windows/Linux). No Mac seria _%l.
        public static void SpawnLandMark()
        {
            GameObject landMarkPrefab = Resources.Load<GameObject>("UI/LAND_MARK_NODE");
            if (landMarkPrefab)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(landMarkPrefab);
                Camera sceneCamera = SceneView.lastActiveSceneView.camera;
                Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0f);
                Vector3 worldPosition = sceneCamera.ViewportToWorldPoint(viewportCenter);
                worldPosition.y = 0;
                instance.transform.position = worldPosition;
                if (Selection.activeGameObject)
                    instance.transform.parent = Selection.activeGameObject.transform;
                Selection.activeGameObject = instance;

                Debug.LogWarning($"[LandMarkMenu] LandMark NODE spawned at {worldPosition} position.");
                AddLandMarkNode(instance.GetComponent<LandMarkNode>());
            }
            else
                Debug.LogWarning("LandMark prefab not found! Verify if the path 'UI/LAND_MARK_NODE' is correct.");
        }

        public static void AddLandMarkNode(LandMarkNode node)
        {
            SceneFeatureXR feature = ObjectDeletionChecker.FindSceneFeatureInScene();
            if (feature)
            {
                var landMarks = feature.landMarks;
                LandMark landMark = new LandMark();
                landMark.node = node;
                Array.Resize(ref landMarks, landMarks.Length + 1);
                landMarks[landMarks.Length - 1] = landMark;
                feature.landMarks = landMarks;
                node.gameObject.name = $"LandMark{landMarks.Length - 1}";
                ArrangeLandMark(node.transform);
            }
            else
            {
                Debug.LogWarning("SceneFeature not founded in scene.");
            }
        }

        public static void ArrangeLandMark(Transform nodeObject)
        {
            GameObject root = GameObject.Find(ROOT_NAME);
            if (!root)
            {
                root = new GameObject();
                root.name = ROOT_NAME;
            }

            GameObject landMarksRoot = GameObject.Find(LAND_MARK_ROOT_NAME);
            if (!landMarksRoot)
            {
                landMarksRoot = new GameObject();// GameObject.Instantiate(new GameObject(),new Vector3(0,100,0),Quaternion.identity);
                landMarksRoot.name = LAND_MARK_ROOT_NAME;
                landMarksRoot.transform.parent = root.transform;
            }

            nodeObject.parent = landMarksRoot.transform;
        }

    }


    [InitializeOnLoad]
    public class ObjectDeletionChecker
    {

        static ObjectDeletionChecker()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private static void OnHierarchyChanged()
        {

            SceneFeatureXR feature = FindSceneFeatureInScene();

            if (!feature || feature.landMarks == null) return;

            LandMarkNode[] currentLandMarks = UnityEngine.Object.FindObjectsOfType<LandMarkNode>();

            bool added = (currentLandMarks.Length > feature.landMarks.Length);


            if (added)
            {
                for (int i = currentLandMarks.Length - 1; i >= 0; i--)
                {
                    LandMarkNode curr = currentLandMarks[i];
                    //if (curr.transform.parent == null || curr.transform.parent.name != LandMarkMenu.LAND_MARK_ROOT_NAME)
                    LandMarkMenu.ArrangeLandMark(curr.transform);
                    bool found = false;
                    foreach (var prev in feature.landMarks) { if (prev.node == curr) { found = true; break; } }
                    if (!found)
                    {
                        LandMarkMenu.AddLandMarkNode(curr);
                    }
                }

            }
            else

                if(feature.landMarks != null)
                foreach (var prev in feature.landMarks)
                {
                    bool found = false;

                    foreach (var curr in currentLandMarks) { if (prev.node == curr) { found = true; break; } }

                    if (!found)
                    {
                        List<LandMark> tempLandMarks = feature.landMarks.ToList();
                        tempLandMarks.Remove(prev);
                        feature.landMarks = tempLandMarks.ToArray();

                    }
                }



        }


        public static SceneFeatureXR FindSceneFeatureInScene()
        {
            // Seek for SceneFeature component in all GameObjects in scene
            foreach (var obj in UnityEngine.Object.FindObjectsOfType<SceneFeatureXR>())
            {
                return obj; // Returns the first founded
            }
            return null;
        }


    }


    [InitializeOnLoad]
    public class ShortCutsHandler
    {
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void ScriptsHasBeenReloaded()
        {
            SceneView.duringSceneGui += DuringSceneGui;
        }

        private static void DuringSceneGui(SceneView sceneView)
        {
            Event e = Event.current;


            //if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.L) { LandMarkMenu.SpawnLandMark(); }

        }
    }

    public class TwinnySceneMenu
    {



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
        private static void SetPlatformScenes(string platform) {
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


            if(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destinyPath) == null) 
            if (!AssetDatabase.CopyAsset(originalPlatformStartScene, destinyPath))
            {
                Debug.LogError($"Failed to copy '{originalPlatformStartScene}' to '{destinyPath}.");
                return;
            }

            newScenes.Add(new EditorBuildSettingsScene(destinyPath, true));

            destinyPath = Path.Combine(platformFolder, $"{platform}MockupScene.unity");
            // Copia a cena de dentro do pacote para o destino


            if(AssetDatabase.LoadAssetAtPath<UnityEngine.Object> (originalPlatformMockupScene) != null)
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

        [MenuItem("Twinny/Platforms/Create Meta Quest Platform Preset")]
        public static void CreateMetaPreset()
        {
            CreateRuntimePreset<RuntimeXR>();
        }


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

        public static void CreateRuntimePreset<T>() where T : TwinnyRuntime 
        {
            T preset = ScriptableObject.CreateInstance<T>();


            if (AssetDatabase.IsValidFolder("Resources")){
                AssetDatabase.CreateFolder("Assets","Resources");
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
            root.name = LandMarkMenu.ROOT_NAME;
            SceneFeatureXR feature = root.AddComponent<SceneFeatureXR>();
            feature.sceneType = type;
            GameObject world = new GameObject();
            world.name = LandMarkMenu.WORLD_NAME;
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

    public class LanguageTableMenu
    {
        public const string LOCALIZATION_PATH = "Assets/Settings/Language/";

        // MenuItem para criar um novo Language Table
        [MenuItem("Twinny/Localization/New Language Table")]
        private static void CreateNewLanguageFile()
        {
            string path = LOCALIZATION_PATH + "NewLanguageTable.asset";

            // Verifica se a pasta existe, se não, cria
            if (!Directory.Exists(LOCALIZATION_PATH))
            {
                Directory.CreateDirectory(LOCALIZATION_PATH);
                AssetDatabase.Refresh();  // Atualiza o AssetDatabase para incluir a nova pasta
            }

            string referenceAssetPath = "Packages/com.twinny.twe25/Runtime/Scripts/Tables/Languages/PT-BR.asset"; // Caminho do arquivo de referência
            LanguageTable referenceTable = AssetDatabase.LoadAssetAtPath<LanguageTable>(referenceAssetPath);

            if (referenceTable == null)
            {
                Debug.LogError("[LanguageTableMenu] Reference table not found at: " + referenceAssetPath);
                return; // Se o arquivo de referência não existir, o processo é interrompido
            }

            // Cria uma nova instância do LanguageTable
            LanguageTable newLanguageTable = ScriptableObject.CreateInstance<LanguageTable>();

            // Copia os dados do arquivo de referência para o novo asset
            newLanguageTable.stringEntries = referenceTable.stringEntries; // Copiando os entries de strings
            // Cria e salva o novo asset na pasta especificada
            AssetDatabase.CreateAsset(newLanguageTable, path);

            // Salva o asset no disco
            AssetDatabase.SaveAssets();

            // Seleciona o asset criado na janela Project
            EditorUtility.FocusProjectWindow();

            // Log para confirmar a criação
            Debug.LogWarning("New Language File created in: " + path);

            // Seleciona o diretório onde a cena foi salva
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            Selection.activeObject = asset;

            // Mostra a pasta na aba Project
            EditorGUIUtility.PingObject(asset);
        }
    }

}


#endif