using Twinny.UI;
using UnityEngine;

namespace Twinny.System.Network
{
    public class NetworkRuntime : TwinnyRuntime
    {

        [HideInInspector] public bool restarting;

        [Header("Experience")]
        public bool startSinglePlayer = false;

        
        
        [Info("Seconds")]
        [Tooltip("Wait X seconds for connection until start single player experience.")]
        public float connectionTimeout = 10;
        public bool tryReconnect = true;
        [Info("Seconds")]
        [Tooltip("When recenter and application was disconnected, wait X seconds until to restart experience.")]
        public float resetExperienceDelay = 0;

    }
}
