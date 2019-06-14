using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadNetwork : MonoBehaviour
{
    public bool showLines;
    public bool showDetailed;

    private void Reset()
    {
        showLines = false;
        showDetailed = false;
    }
}
