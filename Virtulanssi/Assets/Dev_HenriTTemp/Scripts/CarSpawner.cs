using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject car;
    private CPUCarDrive cDrive;

    private void Awake()
    {
        cDrive = car.GetComponent<CPUCarDrive>();
        Nodes n = RandomStartNode();
        car.transform.position = new Vector3(n.transform.position.x, 0.6f, n.transform.position.z);
        cDrive.previousNode = n;
        Vector3 dir = (n.OutNodes[0].transform.position - n.transform.position).normalized;
        car.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Nodes RandomStartNode()
    {
        Nodes n = null;
        Nodes[] allNodes = FindObjectsOfType<Nodes>();
        while (true)
        {
            int i = Random.Range(0, allNodes.Length - 1);
            if (!allNodes[i].ConnectNode)
            {
                if (allNodes[i].OutNodes != null || allNodes[i].OutNodes.Length != 0)
                {
                    n = allNodes[i];
                    break;
                }
            }
        }

        return n;
    }
}
