using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Twinny.System
{

    [Serializable]
    [CreateAssetMenu(fileName = "ProjectList", menuName = "Twinny/ProjectList")]
    public class ProjectList : ScriptableObject
    {
        public ProjectInfo[] projects;
    }

    [Serializable]
    public class ProjectInfo
    {
        public string addressableKey;
        public string displayName;
        public string displaySub;
        public string description;

        [JsonIgnore]
        public Texture2D thumbnail;
        [HideInInspector]
        public string thumbnailURL;
        [JsonIgnore]
        public Texture2D[] galery;
        [HideInInspector]
        public string[] imageGaleryURL;
    }

}