using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalSceneTalkData : Singleton<GalSceneTalkData>
{
    public string charatorName;
    public string content;

    public void SetData(string name ,string content)
    {
        charatorName = name;
        this.content = content;
    }
}
