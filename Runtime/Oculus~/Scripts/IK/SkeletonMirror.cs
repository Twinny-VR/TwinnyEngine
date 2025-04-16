using System.Collections.Generic;
using UnityEngine;

namespace Twinny.Avatars
{


    public class SkeletonMirror : MonoBehaviour
    {
        private const string IGNORE_LAYER_NAME = "IgnoreMirror";
        [SerializeField] private Transform _sourceSkeleton;
        [SerializeField] private Transform[] _sourceSkeletonBones;
        [SerializeField] private Transform _targetSkeleton;
        [SerializeField] private Transform[] _targetSkeletonBones;


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_sourceSkeleton == null || _sourceSkeleton == null) return;
            _sourceSkeletonBones = GetBones(_sourceSkeleton, IGNORE_LAYER_NAME);
            _targetSkeletonBones = GetBones(_targetSkeleton, IGNORE_LAYER_NAME);
        }
#endif
        private void Start()
        {
            if (_sourceSkeleton == null || _sourceSkeleton == null) return;
            _sourceSkeletonBones = GetBones(_sourceSkeleton, IGNORE_LAYER_NAME);
            _targetSkeletonBones = GetBones(_targetSkeleton, IGNORE_LAYER_NAME);
        }
        void Update()
        {
            if (_sourceSkeleton && _targetSkeleton)
                MirrorSkeleton(_sourceSkeleton, _targetSkeleton);
        }

        Transform[] GetBones(Transform root, string ignoreLayerName)
        {
            var ignoreLayer = LayerMask.NameToLayer(ignoreLayerName);

            var bones = root.GetComponentsInChildren<Transform>();
            List<Transform> boneList = new List<Transform>();

            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i].gameObject.layer == ignoreLayer) continue;
                boneList.Add(bones[i]);
            }
            return boneList.ToArray();
        }

        void MirrorSkeleton(Transform source, Transform target)
        {
            if (source == null || target == null) return;

            for (int i = 0; i < _sourceSkeletonBones.Length; i++)
            {
                Transform sourceBone = _sourceSkeletonBones[i];
                Transform targetBone = _targetSkeletonBones[i];
                targetBone.position = sourceBone.position;
                targetBone.rotation = sourceBone.rotation;
                targetBone.localScale = sourceBone.localScale;
            }
        }



    }



}