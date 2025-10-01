using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Video;

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
        [JsonIgnore]
        public bool uploadThis = true;

        public string addressableKey;
        public string displayName;
        public string displaySub;
        public string description;

        [JsonIgnore]
        public Texture2D thumbnail;
        [HideInInspector]
        public string thumbnailURL;
        [JsonIgnore]
        public VideoClip video;
        [HideInInspector]
        public string videoURL;
        [JsonIgnore]
        public Texture2D[] galery;
        [HideInInspector]
        public string[] imageGaleryURL;

    }
        
}