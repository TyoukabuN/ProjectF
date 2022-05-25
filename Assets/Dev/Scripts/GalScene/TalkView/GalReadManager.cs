using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GalReadManager : Singleton<GalReadManager>
{
    public GalReadItem gri;     //应该用作存档类？
    public int currentIndex = 1;
    string[] configStrArray = { };

    private string fullContent;
    public string cPath = Application.dataPath + "/Dev/Scripts/GalScene/TalkView/config/testConfig.csv";
    public GalReadManager()
    {
        if (gri == null)
        {
            gri = new GalReadItem();
        }
    }
    public string ReadLine(int index)
    {
        if (File.Exists(cPath)&& configStrArray.Length < 1)
        {
            FileStream fs =  File.Open(cPath, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            fullContent = sr.ReadToEnd();
            fs.Close();
            sr.Close();
            configStrArray = fullContent.Split('|');
        }
        return configStrArray[index];
    }

    public void DealString(string str)
    {
        string[] strArray = str.Split(',');
        gri.name = strArray[0];
        gri.content = strArray[1];
    }

    /// <summary>
    /// 专门外部用
    /// </summary>
    public void ReadNext()
    {
        DealString(ReadLine(currentIndex));
        GalSceneTalkData.instance.charatorName = gri.name;
        GalSceneTalkData.instance.content = gri.content;
        currentIndex++;
        if (currentIndex >= configStrArray.Length - 1)
        {
            currentIndex = 1;
        }
    }
}
