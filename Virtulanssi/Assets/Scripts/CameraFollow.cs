using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    // Updates the camera position and rotation according to the target
    void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.rotation = target.rotation;
        if (gameObject.name == "BackCamera")
        {
            transform.LookAt(target);
        }
    }
}
