using UnityEngine;

public class DevCameraFollow : MonoBehaviour
{
    public GameObject target;
    public Transform centerPoint;

    private Vector3 offset;
    private float mouseY, mouseX;

    private float zoom;
    public float zoomSpeed = 4, zoomMin = 0f, zoomMax = -30f;


    // Start is called before the first frame update
    void Start()
    {
        zoom = -3;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        zoom += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;


        if (zoom > zoomMin)
            zoom = zoomMin;

        if (zoom < zoomMax)
            zoom = zoomMax;

        transform.transform.localPosition = new Vector3(0, 0, zoom);

        if (Input.GetMouseButton(1))
        {
            mouseX += Input.GetAxis("Mouse X") *2;
            mouseY -= Input.GetAxis("Mouse Y") *2;
            Debug.Log("Pressing mouse button");
        }


        transform.LookAt(centerPoint);
        centerPoint.localRotation = Quaternion.Euler(mouseY, mouseX, 0);

    }

}
