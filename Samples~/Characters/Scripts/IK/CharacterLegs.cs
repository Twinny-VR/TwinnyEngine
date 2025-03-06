using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinny.XR
{

public class CharacterLegs : MonoBehaviour
{
        [SerializeField]
    private Animator _animator;

    [SerializeField]
    private float footOffset = 0;

    private void Start()
    {
            if(!_animator)
        _animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        AvatarIKGoal[] feet = new AvatarIKGoal[] { AvatarIKGoal.LeftFoot, AvatarIKGoal.RightFoot };
        foreach(AvatarIKGoal foot in feet)
        {
            Vector3 footPosition = _animator.GetIKPosition(foot);
            RaycastHit hit;
            Physics.Raycast(footPosition + Vector3.up, Vector3.down, out hit);
            _animator.SetIKPositionWeight(foot, 1);
            _animator.SetIKPosition(foot, hit.point + new Vector3(0, footOffset, 0));
        }
    }
}

}