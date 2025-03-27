using System;
using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using UnityEngine;

namespace Twinny.System
{
    /// <summary>
    /// Scene layout type VR(Virtual), MR(Mixed), MOBILE(Mobile)
    /// </summary>
    [Serializable]
    public enum SceneType
    {
        VR,//Virtual Reallity
        MR,//Mixed Reallity
        MOBILE //Mobile
    }

    public abstract class SceneFeature : TSingleton<SceneFeature>
    {
        // Start is called before the first frame update
        protected virtual void Start()
        {
            Init();        
        }


        public virtual void TeleportToLandMark(int landMarkIndex) { }

        protected virtual void SetHDRI(Material hdri)
        {
#if NETWORK

            if (hdri == null) hdri = NetworkedLevelManager.Config.defaultSkybox;
#else
            if (hdri == null) hdri = LevelManager.Config.defaultSkybox;
#endif
            if (RenderSettings.skybox != hdri)
                    {
                        RenderSettings.skybox = hdri;
                        DynamicGI.UpdateEnvironment();
                    }


        }

    }
}
