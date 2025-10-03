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

/*
        public ProjectMeta[] projectGroups;
    [Serializable]
    public class ProjectMeta
    {
        [JsonIgnore]
        public ProjectInfo projectInfo;
        public string projectName;
        public string addressableKey; 
        public string version; 
    }
*/
    [Serializable]
    [CreateAssetMenu(fileName = "project_info", menuName = "Twinny/Projects/Project Info Preset")]
    public class ProjectInfo : ScriptableObject
    {
        public string version;
        public string addressableKey;
        public string projectName;
        public string authorName;
        public string description;
        public string info;
        public Texture2D thumbnail;
        public VideoClip video;
        public Texture2D[] galery;
    }

/*

    [Serializable]
    public class ProjectInfoOLD
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
  */      
}