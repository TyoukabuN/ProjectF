using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalSceneTalkController : MonoSingleton<GalSceneTalkController>
{
    // Start is called before the first frame update
    public GalSceneTalkView gstv;
    void Start()
    {
        gstv.nameTxt.text = GalSceneTalkData.instance.charatorName;
        gstv.contentTxt.text = GalSceneTalkData.instance.content;
    }
    private void OnEnable()
    {
        gstv.saveBtn.onClick.AddListener(() => { });
        gstv.clickNextBtn.onClick.AddListener(() => { 
            GalReadManager.instance.ReadNext();
            gstv.nameTxt.text = GalSceneTalkData.instance.charatorName;
            gstv.SetTweenText( GalSceneTalkData.instance.content);
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDisable()
    {
        gstv.saveBtn.onClick.RemoveAllListeners();
        gstv.clickNextBtn.onClick.RemoveAllListeners();
    }
}
