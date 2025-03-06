#if OCULUS && FUSION2

using Fusion;
using Twinny.XR;
using UnityEngine;


namespace Twinny.System.Network
{

    
    public class NetworkPlayer : MonoBehaviour
    {

       // private IKFollowRigVR _ikFollowRigVR;
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
            //PlayerSetup();
        }

        

        public void OnSkeletonAvatarLoaded()
        {
            Debug.LogWarning("SKELETON LOADED");

        }

        /*
        private void PlayerSetup()
        {
            _ikFollowRigVR = GetComponent<IKFollowRigVR>();

            _ikFollowRigVR.Head.vrTarget = LevelManagerXR.Instance.ikTargetHead;
            _ikFollowRigVR.LeftHand.vrTarget = LevelManagerXR.Instance.ikTargetLeftHand;
            _ikFollowRigVR.RightHand.vrTarget = LevelManagerXR.Instance.ikTargetRightHand;

        }
        */
    }

}

#endif