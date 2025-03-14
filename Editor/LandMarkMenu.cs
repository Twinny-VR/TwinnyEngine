#if UNITY_EDITOR && OCULUS
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

}


#endif