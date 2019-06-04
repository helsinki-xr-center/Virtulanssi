using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    // Updates the camera position and rotation according to the target
    void LateUpdate()
    {
        transform.position = target.position + (target.rotation * offset);
        transform.rotation = target.rotation;
    }
}
