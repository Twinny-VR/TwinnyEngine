using System;
using System.Collections;
using Concept.UI;
using UnityEngine;

namespace Twinny.System
{

    [Serializable]
    public class TwinnyRuntime : ScriptableObject
    {
        private static TwinnyRuntime m_instance;

        public bool isTestBuild = true;
        public float fadeTime = 1f;
        public Material defaultSkybox;
        public bool forceFrameRate;
        [ShowIf("forceFrameRate")]
        [Range(60,120)]
        public int targetFrameRate = 90;


        private static TwinnyRuntime LoadRuntimeProfile<T>(string fileName = "") where T : TwinnyRuntime
        {

            if (fileName == "")
            {
                fileName = typeof(T).Name + "Preset";
            }

            var config = Resources.Load<T>(fileName);

            if (config == null)
            {
                Debug.LogError($"[TwinnyManager] Impossible to load '{fileName}'.");
                return null;
            }
            Debug.Log($"[TwinnyManager] RuntimeProfile '{fileName}' loaded successfully.");
            return config;
        }


        public static T GetInstance<T>() where T : TwinnyRuntime
        {
            if( m_instance == null ) m_instance = LoadRuntimeProfile<T>();
            return m_instance as T;
        }


    }
}
