#if OCULUS && FUSION2
using System;
using Fusion;
using UnityEngine;
using UnityEngine.Animations.Rigging;


namespace Twinny.XR
{

public class HeadBodyRig : AvatarRig
    {
    [SerializeField]
    private float _idleHandsSmoothSpeed = 5f;
    public Transform headConstraint;
    Vector3 offset;

    public float turnFactor = 1f;
    public ForwardAxis forwardAxis;

    public enum ForwardAxis
    {
        blue,
        green,
        red
    }

    void Start()
    {
        offset = transform.position - headConstraint.position;
    }

    void FixedUpdate()
    {

            if (!HasStateAuthority) return;
     
                if (leftHand.VRTarget.position == Vector3.zero)//Lost left hand
                {
                    if (leftHand.idleRig.weight < 1f)
                    {
                        float currentLeftWeight = leftHand.idleRig.weight;
                        leftHand.idleRig.weight = Mathf.Lerp(currentLeftWeight, 1f, _idleHandsSmoothSpeed * Time.fixedDeltaTime);
                    }
                }
                else
                {
                    if (leftHand.idleRig.weight > 0f)
                    {
                        float currentLeftWeight = leftHand.idleRig.weight;
                        leftHand.idleRig.weight = Mathf.Lerp(currentLeftWeight, 0, _idleHandsSmoothSpeed * Time.fixedDeltaTime);
                    }
                }


                if (rightHand.VRTarget.position == Vector3.zero)//Lost right hand
                {
                    if (rightHand.idleRig.weight < 1f)
                    {
                        float currentRightWeight = rightHand.idleRig.weight;
                        rightHand.idleRig.weight = Mathf.Lerp(currentRightWeight, 1f, _idleHandsSmoothSpeed * Time.fixedDeltaTime);
                    }
                }
                else
                {
                    if (rightHand.idleRig.weight > 0f)
                    {
                        float currentRightWeight = rightHand.idleRig.weight;
                        rightHand.idleRig.weight = Mathf.Lerp(currentRightWeight, 0, _idleHandsSmoothSpeed * Time.fixedDeltaTime);
                    }
                }

        transform.position = headConstraint.position + offset;
        Vector3 projectionVector = headConstraint.up;
        switch (forwardAxis)
        {
            case ForwardAxis.green:
                projectionVector = headConstraint.up;
                break;
            case ForwardAxis.blue:
                projectionVector = headConstraint.forward;
                break;
            case ForwardAxis.red:
                projectionVector = headConstraint.right;
                break;
        }
        transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(projectionVector, Vector3.up).normalized, Time.deltaTime * turnFactor);

        head.Map();
        rightHand.Map();
        leftHand.Map();
    }
}

}

#endif