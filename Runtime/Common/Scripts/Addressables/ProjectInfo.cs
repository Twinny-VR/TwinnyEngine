using System;
using UnityEngine;

namespace Twinny.Addressables
{
    [Serializable]
    [CreateAssetMenu(fileName = "project_info", menuName = "Twinny/Projects/Project Info Preset")]
    public class ProjectInfo : ScriptableObject
    {
        public string version;
        public string addressableUrl = "[Default]";
        public string addressableCatalogName = "[DefaultCatalogName]";
        public string addressableKey = "[ProjectName]";
        public string projectName;
        public string authorName;
        public string description;
        public string info;
        public string thumbnailRef;
        public string videoRef;
        public string[] galeryRef;


        public string GetCatalogName()
        {
            if (addressableCatalogName.ToLowerInvariant() == "[defaultcatalogname]")
                return $"catalog_{version}.json";
            else
            {
                if (!addressableCatalogName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    return addressableCatalogName + ".json";
                else 
                    return addressableCatalogName;
            }
        }
        //public AssetReferenceTexture2D thumbnail;
        //public AssetReferenceT<VideoClip> video;
        //public AssetReferenceTexture2D[] galery;
    }
}