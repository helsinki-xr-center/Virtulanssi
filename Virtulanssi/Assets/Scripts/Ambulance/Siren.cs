using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Siren : MonoBehaviour
{
    private bool isPlaying = false;
    public AudioSource siren;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            isPlaying = !isPlaying;
            if (isPlaying == true)
            {
                siren.Play();
            }
            else
            {
                siren.Pause();
            }
        }
    }
}
