using System;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace Twinny.Addressables
{
    [Serializable]
    [CreateAssetMenu(fileName = "project_list", menuName = "Twinny/Projects/Project List Preset")]
    public class ProjectList : ScriptableObject
    {
        public string version;
        public string[] projectGroups;
    }
}