#if OCULUS && FUSION2
using System.Collections;
using UnityEngine;


namespace Twinny.System.Network
{
    public class AlignmentManager : MonoBehaviour
    {
        [SerializeField] private ColocationManager _colocationManager;


        private Transform _cameraRigTransform;


        private void Awake()
        {
            _cameraRigTransform = FindAnyObjectByType<OVRCameraRig>().transform;
        }


        public void AlignUserToAnchor(OVRSpatialAnchor anchor, Vector3 offsetPosition, float offsetRotationY)
        {
            if(!anchor || !anchor.Localized)
            {
                Debug.LogError("[AlignmentManager] Invalid or unlocalized anchor. Cannot align.");
                return;
            }
                Debug.LogWarning($"[AlignmentManager] Starting alignment to anchor {anchor.Uuid}.");

            StartCoroutine(AlignmentCoroutine(anchor, offsetPosition, offsetRotationY));
                
        }


        private IEnumerator AlignmentCoroutine(OVRSpatialAnchor anchor, Vector3 offsetPosition, float offsetRotationY)
        {
            Transform safeArea = AnchorManager.Instance.transform;


            if (safeArea == null) {
                Debug.LogWarning("[AlignmentManager] AnchorManager not found.");
                yield break;
            }

            var anchorTransform = anchor.transform;
            Debug.LogWarning($"ANCHOR: {anchorTransform.position} ROT:{anchorTransform.eulerAngles.y} EU:{anchorTransform.eulerAngles.y}");


            for (var count = 2; count > 0 ; count--)
            {
                _cameraRigTransform.position = Vector3.zero;
                _cameraRigTransform.eulerAngles = Vector3.zero;
                safeArea.position = Vector3.zero;
                safeArea.eulerAngles = Vector3.zero; 
                
                yield return null;

                _cameraRigTransform.position = anchorTransform.InverseTransformPoint(Vector3.zero);
                _cameraRigTransform.eulerAngles = new Vector3(0,-anchorTransform.eulerAngles.y,0);


                               
                safeArea.eulerAngles = new Vector3(0, offsetRotationY, 0);
                safeArea.position = offsetPosition;
                AnchorManager.CreateAnchor();
                //safeArea.position = anchorTransform.TransformPoint(_cameraRigTransform.position - _colocationManager.offsetAnchorPosition);


                Debug.LogWarning($"[AlignmentManager] Aligned Camera Rig Position: {_cameraRigTransform.position}, Rotation: {_cameraRigTransform.eulerAngles.y}.");
                Debug.LogWarning($"[AlignmentManager] Aligned Safe Area Position: {safeArea.position}, Rotation: {safeArea.eulerAngles.y}.");

                yield return new WaitForEndOfFrame();
            }

                Debug.LogWarning("[AlignmentManager] Aligment Complete!");
        }



    }
}
#endif