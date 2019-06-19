using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideMirrorMeshGenerator : MonoBehaviour
{
    public float zero, oneX, twoX, threeX, fourX, fiveX, sixX, sevenX, eightX, nineX, tenX, elevenX;
    public float oneY, twoY, threeY, fourY, fiveY, sixY, sevenY, eightY, nineY, tenY, elevenY;

    // Start is called before the first frame update
    void Update()
    {
        Mesh leftMesh = new Mesh();

        Vector3[] leftVertices = new Vector3[12];

        leftVertices[0] = new Vector3(zero, zero);
        leftVertices[1] = new Vector3(oneX, oneY);
        leftVertices[2] = new Vector3(twoX, twoY);
        leftVertices[3] = new Vector3(threeX, threeY);
        leftVertices[4] = new Vector3(fourX, fourY);
        leftVertices[5] = new Vector3(fiveX, fiveY);
        leftVertices[6] = new Vector3(sixX, sixY);
        leftVertices[7] = new Vector3(sevenX, sevenY);
        leftVertices[8] = new Vector3(eightX, eightY);
        leftVertices[9] = new Vector3(nineX, nineY);
        leftVertices[10] = new Vector3(tenX, tenY);
        leftVertices[11] = new Vector3(elevenX, elevenY);

        leftMesh.vertices = leftVertices;

        leftMesh.triangles = new int[] {0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 6, 0, 6, 7, 0, 7, 8, 0, 8, 9, 0, 9, 10, 0, 10, 11, 0, 11, 1};

        GetComponent<MeshFilter>().mesh = leftMesh;
    }

}
