using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationLagController : MonoBehaviour
{

    [SerializeField] private Transform followTransform;  // The camera transform
    [SerializeField] private float lagSpeed = 5.0f;   // Adjust this to control the lag speed
    [SerializeField] private float smooth = 8;

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * lagSpeed;
        float mouseY = Input.GetAxisRaw("Mouse Y") * lagSpeed;

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRot = rotationX * rotationY;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, smooth * Time.deltaTime);
    }
}
