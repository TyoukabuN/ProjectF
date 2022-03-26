using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class Controller
{
    //time interval of key down
    private Dictionary<KeyCode, float> keyPressBook = new Dictionary<KeyCode, float>();
    private List<KeyRecord> keyRecordList = new List<KeyRecord>();

    private int recordMaxCount = 500;
    private int recordMinCount = 50;

    private List<KeyCode> observedKeyList = new List<KeyCode>();

    public void InitObservedKey()
    {
        foreach (var field in typeof(InputDefine).GetFields())
        {
            var attrs = field.GetCustomAttributes(typeof(ObservedKey), false);
            if (attrs.Length <= 0)
            {
                continue;
            }

            KeyCode key = (KeyCode)field.GetValue(field);
            observedKeyList.Add(key);
        }
    }

    public bool Debug_Input = false;
    public void DebugInput(KeyCode keyCode,string exStr = "")
    {
        if (Debug_Input)
        { 
            //Debug.Log(keyCode + exStr);
        }
    }
    public void Update_InputDetection()
    {
        foreach (var keyCode in observedKeyList)
        {
            if (Input.GetKeyDown(keyCode) || Input.GetKey(keyCode))
            {
                if (!GetKey(keyCode))
                {
                    DebugInput(keyCode, " Down");
                    RecordKey(keyCode);
                }
            }
            if(Input.GetKeyUp(keyCode) || !Input.GetKey(keyCode))
            {
                if (GetKey(keyCode))
                { 
                    DebugInput(keyCode, " Up");
                    ClearKey(keyCode);
                }
            }
        }
    }
    /// <summary>
    /// 是否点击了按钮
    /// </summary>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public bool GetKey(KeyCode keyCode)
    {
        if (!keyPressBook.ContainsKey(keyCode))
        {
            keyPressBook[keyCode] = 0;
            return false;
        }

        return keyPressBook[keyCode] > 0;
    }

    public bool GetKeyDown(KeyCode keyCode)
    {
        return Input.GetKeyDown(keyCode);
    }

    public bool GetKeyUp(KeyCode keyCode)
    {
        return Input.GetKeyUp(keyCode);
    }

    /// <summary>
    /// 案件记录
    /// </summary>
    /// <param name="keyCode"></param>
    public void RecordKey(KeyCode keyCode)
    {
        keyRecordList.Add(new KeyRecord(keyCode, Time.time));
        keyPressBook[keyCode] = 1;

        //clear
        if (keyRecordList.Count >= recordMaxCount)
        {
            keyRecordList.RemoveRange(0, recordMaxCount - recordMinCount);
        }
    }
    public void ClearKey(KeyCode keyCode)
    {
        keyPressBook[keyCode] = 0;
    }

    public void ClearAllKeyRecord()
    {
        keyPressBook = new Dictionary<KeyCode, float>();
        keyRecordList = new List<KeyRecord>();
    }

    /// <summary>
    /// 是否连击了
    /// </summary>
    /// <param name="keyCode"></param>
    /// <returns></returns>
    public bool IsDoubleClick(KeyCode keyCode)
    {
        return SameKeyComboDetection(keyCode, 2);
    }

    /// <summary>
    /// 连击的间隔
    /// </summary>
    [SerializeField]private float comboInterval = 0.1f;

    /// <summary>
    /// 相同按钮的连击判断
    /// </summary>
    /// <param name="keyCode">按键</param>
    /// <param name="combo">连击数</param>
    /// <returns></returns>
    public bool SameKeyComboDetection(KeyCode keyCode,int combo)
    {
        int click = 0;
        for (int i = keyRecordList.Count - 1; i >= 0; i--)
        {
            var record = keyRecordList[i];
            //time over return
            if (Time.time - record.time > comboInterval * combo)
            {
                return click >= combo;
            }

            if (record.keyCode == keyCode)
            {
                click++;
            }
        }
        return click >= combo;
    }

    public struct KeyRecord
    {
        public KeyCode keyCode;
        public float time;

        public KeyRecord(KeyCode keyCoded, float time)
        {
            this.keyCode = keyCoded;
            this.time = time;
        }
    }
}

[AttributeUsage( AttributeTargets.All)]
public sealed class ObservedKey : Attribute
{ 

}
