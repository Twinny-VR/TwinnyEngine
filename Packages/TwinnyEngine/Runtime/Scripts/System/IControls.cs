using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using Twinny.Helpers;
using Twinny.System.Local;
using Twinny.System.Network;
using Twinny.UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace Twinny.System
{
    /// <summary>
    /// This interface used to control Level Scene actions.
    /// </summary>
    [Obsolete("IControls is deprecated. Use 'NetworkManager' instead.")]
    public interface IControls
    {



        void SetUp();
        Task LoadAdditiveSceneAsync(string scene, int landMarkIndex);
        Task UnloadAdditivesScenes();


    }
}
