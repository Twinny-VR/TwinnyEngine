using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Twinny.System.Network
{

public class NetworkPlayer : MonoBehaviour
{
        private NetworkObject _network;

        private void Awake()
        {
            _network = transform.parent.GetComponent<NetworkObject>();
        }

        // Start is called before the first frame update
        void Start()
    {
            LevelManager.Instance.colocation.SetActive(false);
            LevelManager.Instance.colocation.SetActive(true);
            OVRColocationSession.StartDiscoveryAsync();
        }

}

}