using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    // Updates the camera position according to the target's position + an offset vector
    void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.rotation = target.rotation;
    }
}
