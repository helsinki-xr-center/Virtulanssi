using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCameraFollow : MonoBehaviour
{
    public GameObject target;
    private float delay = 1;
    private Vector3 offset;


    // Offsets the camera from player
    void Start()
    {
        offset = target.transform.position - transform.position;
    }

    // Camera follows the player with a slight delay
    void LateUpdate()
    {
        
        float targetAngle = target.transform.eulerAngles.y;
        float currentAngle = transform.eulerAngles.y;
        float delayedAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * delay);

        Quaternion rotation = Quaternion.Euler(0, delayedAngle, 0);
        transform.position = target.transform.position - (rotation * offset);

        transform.LookAt(target.transform);
    }
}
