using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{

    private GameObject ambulance;
    private GameObject centerpoint;

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
