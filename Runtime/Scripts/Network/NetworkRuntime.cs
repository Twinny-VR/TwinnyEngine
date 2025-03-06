using System.Collections;
using System.Collections.Generic;
using Fusion;
using Twinny.UI;
using UnityEngine;

namespace Twinny.System.Network
{
    public class NetworkRuntime : TwinnyRuntime
    {

        [HideInInspector]
        public bool restarting;

        [Header("Experience")]
        public bool startSinglePlayer = false;

        
        
        [Info("Seconds")]
        [Tooltip("Wait X seconds for connection until start single player experience.")]
        public int connectionTimeout = 10;
        public bool tryReconnect = true;


        [Header("Avatars")]
        public GameObject avatarPrefab;


    }
}
