using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalSceneTalkController : MonoSingleton<GalSceneTalkController>
{
    // Start is called before the first frame update
    void Start()
    {
        GalSceneTalkView.instance.nameTxt.text = GalSceneTalkData.instance.charatorName;
        GalSceneTalkView.instance.contentTxt.text = GalSceneTalkData.instance.content;
    }
    private void OnEnable()
    {
        GalSceneTalkView.instance.saveBtn.onClick.AddListener(() => { });
        GalSceneTalkView.instance.clickNextBtn.onClick.AddListener(() => { 
            GalReadManager.instance.ReadNext();
            GalSceneTalkView.instance.nameTxt.text = GalSceneTalkData.instance.charatorName;
            GalSceneTalkView.instance.nameTxt.text = GalSceneTalkData.instance.charatorName;
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDisable()
    {
        GalSceneTalkView.instance.saveBtn.onClick.RemoveAllListeners();
        GalSceneTalkView.instance.clickNextBtn.onClick.RemoveAllListeners();
    }
}
