#if OCULUS && FUSION2

using UnityEngine;


namespace Twinny.System.Network
{

    
    public class NetworkPlayer : MonoBehaviour
    {

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        // Start is called before the first frame update
        void Start()
        {
            AnchorManager.Recolocation();
            OVRColocationSession.StartDiscoveryAsync();
        }

        public void OnAvatarLoaded()
        {
            Debug.LogWarning("AVATAR LOADED");
            int newLayer = LayerMask.NameToLayer("Character");

            foreach (Transform child in _transform)
            {
                child.gameObject.layer = newLayer;
            }
        }

        public void OnSkeletonAvatarLoaded()
        {
            Debug.LogWarning("SKELETON LOADED");

        }
    }

}

#endif