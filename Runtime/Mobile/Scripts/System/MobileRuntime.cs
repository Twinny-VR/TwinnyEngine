using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Twinny.Addressables;
using UnityEngine;

namespace Twinny.System
{
    [Serializable]
    public class MobileRuntime : TwinnyRuntime
    {
        public static MobileRuntime instance => GetInstance<MobileRuntime>();

        [Space]
        [Header("MOBILE PRESET")]
        public bool autoStart = true;

        [Header("Local Settings")]
        public string localProjectListTempDirectory = "../Temp/ProjectListUpload";

        [Header("ADDRESSABLES Remote Settings")]
        public string hostDirectory = "https://twinnyvr.co/apps/twinny/projects/";
        public string remoteProjectListDirectory = "project_list";
        [Tooltip("The host json file name will not be the same of the 'ProjectList' asset unless you set it.")]
        public string projectListFileName = "projectlist";

        public static string GetRemotePath()
        {
            return Path.Combine(instance.hostDirectory, instance.remoteProjectListDirectory);
        }

        [Obsolete("Use AddressableManager.LoadSeparateAssetAsync instead")]
        public static async Task<ProjectList> GetProjectListAsync() 
        {
            try
            {
                string url = $"{GetRemotePath().TrimEnd('/')}/{instance.projectListFileName}.json";

                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(url);
                    ProjectList list = JsonConvert.DeserializeObject<ProjectList>(json);
                    return list;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileRuntime] Erro ao carregar ProjectList: {ex.Message}");
                return null;
            }
        }
    }
}
