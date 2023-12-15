using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class BarValue : MonoBehaviour{
    [SerializeField] barType barType=barType.Fill;
    [SerializeField] string valueName;
    [ReadOnly][SerializeField] float value;
    [ReadOnly][SerializeField] float maxValue;
    [DisableInPlayMode][SerializeField] bool onlyOnEnable=false;
    [HideInPlayMode][SerializeField] bool onValidate=false;
    void Start(){if(onlyOnEnable)ChangeBar();}
    void OnEnable(){if(onlyOnEnable)ChangeBar();}
    void OnValidate(){if(onValidate)ChangeBar();}
    void Update(){if(!onlyOnEnable)ChangeBar();}
    void ChangeBar(){
        if(valueName=="health"){}
        if(valueName=="energyTimer"){
            // value=CardManager.instance.energyRegenTimer;
            // maxValue=CardManager.instance.energyRegenTime;
            value=CardManager.instance.energyRegenTime-CardManager.instance.energyRegenTimer;
            maxValue=CardManager.instance.energyRegenTime;
        }

        if(barType==barType.HorizontalR){transform.localScale=new Vector2(value/maxValue,transform.localScale.y);}
        if(barType==barType.HorizontalL){transform.localScale=new Vector2(value/maxValue,transform.localScale.y);/*new Vector2(-(value/maxValue),transform.localScale.y);*/}
        if(barType==barType.VerticalU){transform.localScale=new Vector2(transform.localScale.x,-(value/maxValue));}
        if(barType==barType.VerticalD){transform.localScale=new Vector2(transform.localScale.x,value/maxValue);}
        if(barType==barType.Fill){
            if(maxValue>value){
                GetComponent<Image>().fillAmount=value/maxValue;
            }else{
                GetComponent<Image>().fillAmount=maxValue/value;
            }
        }
    }
}
public enum barType{
    HorizontalR,
    HorizontalL,
    VerticalU,
    VerticalD,
    Fill
}