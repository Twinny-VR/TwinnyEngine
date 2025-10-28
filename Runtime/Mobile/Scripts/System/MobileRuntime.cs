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

        public static string GetRemotePath()
        {
            return instance.hostDirectory;
           // return Path.Combine(instance.hostDirectory, instance.remoteProjectListDirectory);
        }




    }
}
