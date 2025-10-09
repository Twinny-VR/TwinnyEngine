using System;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Video;

namespace Twinny.Addressables
{
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
        public string thumbnailRef;
        public string videoRef;
        public string[] galeryRef;

        //public AssetReferenceTexture2D thumbnail;
        //public AssetReferenceT<VideoClip> video;
        //public AssetReferenceTexture2D[] galery;
    }
}