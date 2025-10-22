using System;
using UnityEngine;

namespace Twinny.Addressables
{
    [Serializable]
    [CreateAssetMenu(fileName = "project_scenes", menuName = "Twinny/Projects/Project Scenes Preset")]
    public class ProjectScenes : ScriptableObject
    {
        public ProjectSceneInfo[] sceneInfos;
    }

    [Serializable]
    public class ProjectSceneInfo
    {
        public string sceneName;
        public string sceneDisplayName;
        public string addressableKey;
    }
}
