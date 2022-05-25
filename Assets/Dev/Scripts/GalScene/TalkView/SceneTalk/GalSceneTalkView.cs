using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
public class GalSceneTalkView : MonoBehaviour
{
    public GalSceneTalkData gstd;
    public Text nameTxt;
    public Text contentTxt;
    public Button saveBtn;
    public Button clickNextBtn;

    public void SetTweenText(string str,UnityAction callback = null)
    {
        contentTxt.text = "";
        contentTxt.DORestart();
        contentTxt.DOText(str, 0.5f).OnComplete(() =>
        {
            if (callback != null)
            {
                callback.Invoke();
            }
        });
    }
}
