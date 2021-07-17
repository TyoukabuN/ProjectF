using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstShaderTest : MonoBehaviour
{
    // Start is called before the first frame update
    public Material mar;
    public Color color;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (mar != null)
        {
            color.r = Mathf.Cos(Time.time);
            color.g = Mathf.Sin(Time.time);
            color.b = Mathf.Tan(Time.time);
            mar.SetColor("_Color", color);
        }  
    }
}
