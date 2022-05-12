using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GalScene 
{
    public GalScene beforScene;
    public GalScene nextScene;

    public GalScene value;
    public GameObject gameObject;
    public string name;
    
    public GalScene(GameObject obj)
    {
        this.gameObject = obj;
        this.name = obj.name;
        value = this;
    }

}
