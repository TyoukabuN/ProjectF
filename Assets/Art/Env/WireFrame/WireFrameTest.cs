using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireFrameTest : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        // create new colors array where the colors will be created.
        Color[] colors = new Color[vertices.Length];

        Color[] ColorPref = new Color[3] { Color.red, Color.green, Color.blue, };
        //for (int i = 0; i < vertices.Length; i++)
        //{ 
        //    colors[i] = ColorPref[i%3];
        //}

        int[] book = new int[vertices.Length];
        int[] slopeBook = new int[vertices.Length];
        for (int i = 0; i < book.Length; i ++)
        {
            book[i] = -1;
            //slopeBook[i] = -1;
        }

        int slopeNum = 1;

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        //for (int i = 0; i < 3*24; i += 3)
        {

            for (int k = 0; k < 3; k++)
            {
                int indice = mesh.triangles[i + k];

                if (book[indice] >= 0)
                { 
                    colors[indice].a = slopeNum;
                    continue;
                }

                int colorIdx = 0;

                for (int j = 0; j < 3; j++)
                {
                    if (k != j && colorIdx == book[mesh.triangles[i + j]])
                    {
                        if (++colorIdx > 2)
                        {
                            Debug.Log("GG");
                            break;
                        }
                        j = -1;
                    }
                }
                colors[indice] = ColorPref[colorIdx];
                book[indice] = colorIdx;
            }
            slopeNum+=2;
        }


        // assign the array of colors to the Mesh.
        mesh.colors = colors;
    }
}