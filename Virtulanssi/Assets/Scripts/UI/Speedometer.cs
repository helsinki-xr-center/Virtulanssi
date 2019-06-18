using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    public TextMeshProUGUI number;
    public virtual void ChangeText(float speed)
    {
        float s = speed;
        number.text = Mathf.Round(s) + " ";
    }
}
