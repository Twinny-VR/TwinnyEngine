using System;
using Fusion;
using Twinny.System;
using UnityEngine;

namespace Twinny.XR
{
    /*
    public class IKFollowRigVR : NetworkBehaviour
    {

        private Transform _transform;

        #region SERIALIZE_FIELDS
        [Header("Config")]
        [SerializeField] private float positionLerpRate = 5f;
        [SerializeField] private float rotationLerpRate = 5f;
        [Range(0, 10)]
        [SerializeField] private float turnSmoothness = 0.1f;
        [SerializeField] private Transform _cameraRig;
        [SerializeField] private Transform _mainCamera;
        [SerializeField] private VRMap head;
        [SerializeField] private VRMap leftHand;
        [SerializeField] private VRMap rightHand;
        [SerializeField] private Transform _anchor;
          // [SerializeField] private Vector3 headBodyPositionOffset;
          // [SerializeField] private float headBodyYawOffset;
        #endregion

        #region PRIVATE_FIELDS
          [Networked] private  Vector3 targetHeadPosition { get; set; }
        [Networked] private Quaternion targetHeadRotation { get; set; }
        [Networked] private Vector3 targetLeftHandPosition { get; set; }
        [Networked] private Quaternion targetLeftHandRotation { get; set; }
        [Networked] private Vector3 targetRightHandPosition { get; set; }
        [Networked] private Quaternion targetRightHandRotation { get; set; }
        #endregion

        #region PROPERTIES
        public VRMap Head => head;
        public VRMap LeftHand => leftHand;
        public VRMap RightHand => rightHand;
        #endregion





        #region MonoBehaviour Methods

        void Start()
        {
            _transform = transform;
            _cameraRig = FindObjectOfType<OVRCameraRig>().transform;
            _mainCamera = Camera.main.transform;
         //   _transform.SetParent(HasStateAuthority ? _mainCamera : null);
            AnchorManager.Recolocation();

        }


        private void LateUpdate()
        {
            _anchor = AnchorManager.Instance.transform;
            Quaternion anchorRotation = _anchor.rotation;
            float trig = anchorRotation.eulerAngles.y * Mathf.Deg2Rad;
            
            if (HasStateAuthority)
            {
 /*
                _transform.position = new Vector3((Mathf.Cos(trig) * (head.ikTarget.position.x - _anchor.position.x)) - (Mathf.Sin(trig) * (head.ikTarget.position.z - _anchor.position.z)), head.ikTarget.position.y - 0.5f, (Mathf.Cos(trig) * (head.ikTarget.position.z - _anchor.position.z)) + (Mathf.Sin(trig) * (head.ikTarget.position.x - _anchor.transform.position.x)));
                float yaw = head.vrTarget.eulerAngles.y - anchorRotation.eulerAngles.y;
                _transform.rotation = Quaternion.Lerp(Quaternion.Euler(_transform.eulerAngles.x, yaw, _transform.eulerAngles.z), Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z), Time.deltaTime * turnSmoothness);
*//*
                _transform.position = _mainCamera.position;
                head.Map();
                leftHand.Map();
                rightHand.Map();
            }
            else
            {
                /*
                float yaw2 = targetHeadRotation.eulerAngles.y + anchorRotation.eulerAngles.y;
                head.ikTarget.position = Vector3.Lerp(head.ikTarget.position, targetHeadPosition, Time.deltaTime * positionLerpRate);
                head.ikTarget.rotation = Quaternion.Slerp(head.ikTarget.rotation, Quaternion.Euler(targetHeadRotation.eulerAngles.x, yaw2, targetHeadRotation.eulerAngles.z), Time.deltaTime * rotationLerpRate);

                float yaw3 = targetLeftHandRotation.eulerAngles.y - anchorRotation.eulerAngles.y;
                leftHand.ikTarget.position = Vector3.Lerp(leftHand.ikTarget.position, _transform.position, Time.deltaTime * positionLerpRate);
                leftHand.ikTarget.rotation = Quaternion.Slerp(leftHand.ikTarget.rotation, _transform.rotation, Time.deltaTime * rotationLerpRate);

                float yaw4 = targetRightHandRotation.eulerAngles.y - anchorRotation.eulerAngles.y;
                rightHand.ikTarget.position = Vector3.Lerp(rightHand.ikTarget.position, _transform.position, Time.deltaTime * positionLerpRate);
                rightHand.ikTarget.rotation = Quaternion.Slerp(rightHand.ikTarget.rotation, _transform.rotation, Time.deltaTime * rotationLerpRate);
                *//*
            }
        }
        #endregion
        public override void FixedUpdateNetwork()
        {


            base.FixedUpdateNetwork();

            if (HasStateAuthority)
            {
                 _anchor = AnchorManager.Instance.transform;
                targetHeadPosition = head.ikTarget.position;
                targetHeadRotation = Quaternion.Euler(head.ikTarget.eulerAngles.x, (head.ikTarget.eulerAngles.y - _anchor.rotation.eulerAngles.y), head.ikTarget.eulerAngles.z);
                targetLeftHandPosition = leftHand.ikTarget.position;
                targetLeftHandRotation = leftHand.ikTarget.rotation;
                targetRightHandPosition = rightHand.ikTarget.position;
                targetRightHandRotation = rightHand.ikTarget.rotation;
            }
            else
            {
                head.ikTarget.position = targetHeadPosition;
                head.ikTarget.eulerAngles = targetHeadRotation.eulerAngles;
                leftHand.ikTarget.position = targetLeftHandPosition;
                leftHand.ikTarget.rotation = targetLeftHandRotation;
                rightHand.ikTarget.position = targetRightHandPosition;
                rightHand.ikTarget.rotation = targetRightHandRotation;
            }


        }

        #region SYNC


        #endregion

    }*/
}



