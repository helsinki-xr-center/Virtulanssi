using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Headlights : MonoBehaviour
{
    public GameObject headlights;

    public GameObject highBeamRight;
    public GameObject highBeamLeft;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (!headlights.activeSelf)
            {
                headlights.SetActive(true);
            }

            else if (headlights.activeSelf)
            {
                headlights.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!highBeamRight.activeSelf)
            {
                highBeamRight.SetActive(true);
                highBeamLeft.SetActive(true);
            }
            else if (highBeamRight.activeSelf)
            {
                highBeamRight.SetActive(false);
                highBeamLeft.SetActive(false);
            }

        }
    }
}
