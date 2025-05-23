using System;
using Concept.Helpers;
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
        [SerializeField] private Material _sceneSkyBox;
        public Material sceneSkybox { get => _sceneSkyBox; }
        [NonSerialized]
        public Transform[] interestPoints;


        protected override void Start()
        {
            base.Start();
             SetHDRI(_sceneSkyBox);
        }

        public virtual void TeleportToLandMark(int landMarkIndex) { }

        protected virtual void SetHDRI(Material hdri)
        {
#if NETWORK
            if (hdri == null) hdri = NetworkedLevelManager.Config.defaultSkybox;
#endif
            if (hdri == null) hdri = TwinnyManager.config.defaultSkybox;

            if (RenderSettings.skybox != hdri)
                    {
                        RenderSettings.skybox = hdri;
                        DynamicGI.UpdateEnvironment();
                    }


        }

    }
}
