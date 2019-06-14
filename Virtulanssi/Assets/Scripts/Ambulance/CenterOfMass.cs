using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOfMass : MonoBehaviour
{
    public Vector3 com;
    public Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //rb.centerOfMass = com;
        rb.ResetCenterOfMass();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * com, 1f);
        //Gizmos.DrawSphere(transform.position + transform.rotation * rb.centerOfMass, 1f);
    }
}
