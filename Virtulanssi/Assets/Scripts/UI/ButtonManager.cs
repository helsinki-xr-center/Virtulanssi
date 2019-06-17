using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    private Rigidbody rb;//rigid body of car
    private GameObject ambulance;


    //private GameObject centerpoint;
    private Vector3 reset;

    public void TestButton()
    {
        Debug.Log("Testing button...");
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResetPlayer()
    {

        Vector3 reset = new Vector3(0, 0, 0);
        ambulance = GameObject.Find("Ambulance");
        rb = ambulance.GetComponent<Rigidbody>();
        ambulance.transform.position = reset;
        ambulance.transform.rotation = Quaternion.identity;
        rb.velocity = reset;
    }


    //public void SwitchToVR()
    //{
    //    // Scripts
    //    ambulance = GameObject.Find("Ambulance");
    //    ambulance.GetComponent<AmbulanceControllerWASD>().enabled = false;
    //    ambulance.GetComponent<AmbulanceController>().enabled = true;
    //    ambulance.GetComponent<LogitechSteeringWheel>().enabled = true;

    //    // Cameras
    //    centerpoint = GameObject.Find("CenterPoint");
    //    centerpoint.SetActive(false);
    //}
}
