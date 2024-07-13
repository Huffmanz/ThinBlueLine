using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

[System.Serializable]
public class HumanBone
{
    public HumanBodyBones bone;
    public float weight = 1.0f;
}

public class WeaponIK : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Transform aimTransform;
    [SerializeField] int iterations = 10;
    [SerializeField] float angleLimit = 90.0f;
    [SerializeField] float distanceLimit = 1.5f;
    [Range(0, 1)]
    [SerializeField] float weight = 1.0f;
    [SerializeField] HumanBone[] humanBones;
    Transform[] boneTransforms;

    private void Start()
    {
        Animator animator = GetComponentInChildren<Animator>();
        boneTransforms = new Transform[humanBones.Length];
        for (int i = 0; i < boneTransforms.Length; i++)
        {
            boneTransforms[i] = animator.GetBoneTransform(humanBones[i].bone);
        }
    }

    private void LateUpdate()
    {
        if (aimTransform == null || target == null) return;
        Vector3 targetPosition = GetTargetPosition();
        for (int i = 0; i < iterations; i++)
        {
            for (int b = 0; b < boneTransforms.Length; b++)
            {
                Transform bone = boneTransforms[b];
                float boneWeight = humanBones[b].weight * weight;
                AimAtTarget(bone, targetPosition, boneWeight);
            }
        }
    }

    private void AimAtTarget(Transform bone, Vector3 targetPosition, float weight)
    {
        if (bone == null || targetPosition == null) return;
        Vector3 aimDirection = aimTransform.forward;
        Vector3 targetDirection = targetPosition - aimTransform.position;

        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);
        bone.rotation = blendedRotation * bone.rotation;
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 targetDirection = target.position - aimTransform.position;
        Vector3 aimDirection = aimTransform.forward;
        float blendOut = 0.0f;

        float targetAngle = Vector3.Angle(targetDirection, aimDirection);
        if (targetAngle > angleLimit)
        {
            blendOut += (targetAngle - angleLimit) / 50.0f;
        }

        float targetDistance = targetDirection.magnitude;
        if (targetDistance < distanceLimit)
        {
            blendOut += distanceLimit - targetDistance;
        }

        Vector3 direction = Vector3.Slerp(targetDirection, aimDirection, blendOut);
        return aimTransform.position + direction;
    }

    public void SetTargetTransform(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetAimTransform(Transform newAimTransform)
    {
        aimTransform = newAimTransform;
    }
}
