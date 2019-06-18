using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicators : MonoBehaviour
{
    private IEnumerator RightIndicatorON;
    private IEnumerator LeftIndicatorON;

    public GameObject rightIndicator;
    public GameObject leftIndicator;
    public bool keyPressedRight = false;
    public bool keyPressedLeft = false;

    void Start()
    {
        RightIndicatorON = TurnON(rightIndicator);
        LeftIndicatorON = TurnON(leftIndicator);
    }

    IEnumerator TurnON(GameObject indicator)
    {
        GameObject indi = indicator;
        while (true)
        {
            indi.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            indi.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) /* OR input from steering wheel*/) //Acyivate right indicator
        {
            keyPressedRight = !keyPressedRight;
            if (keyPressedRight)
            {
                StartCoroutine(RightIndicatorON);
                StopCoroutine(LeftIndicatorON);
                leftIndicator.SetActive(false);
                keyPressedLeft = false;
            }
            else
            {
                StopCoroutine(RightIndicatorON);
                rightIndicator.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.U)/* OR input from steering wheel*/) //Activate left indicator
        {
            keyPressedLeft = !keyPressedLeft;
            if (keyPressedLeft)
            {
                StartCoroutine(LeftIndicatorON);
                StopCoroutine(RightIndicatorON);
                rightIndicator.SetActive(false);
                keyPressedRight = false;
            }
            else
            {
                StopCoroutine(LeftIndicatorON);
                leftIndicator.SetActive(false);
            }
        }
    }
}
