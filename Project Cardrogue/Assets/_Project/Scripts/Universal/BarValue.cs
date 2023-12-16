using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class BarValue : MonoBehaviour{
    [SerializeField] barType barType=barType.Fill;
    [SerializeField] string valueName;
    [SerializeField] bool reverse;
    [ReadOnly][SerializeField] float valueN;
    [ReadOnly][SerializeField] float value;
    [ReadOnly][SerializeField] float maxValue;
    [DisableInPlayMode][SerializeField] bool onlyOnEnable=false;
    [HideInPlayMode][SerializeField] bool onValidate=false;
    [HideInEditorMode][SerializeField] bool maxValuesSet=false;
    void Start(){SetMaxValues();if(onlyOnEnable)if(maxValuesSet)ChangeBar();}
    void OnEnable(){if(onlyOnEnable)if(maxValuesSet)ChangeBar();}
    void OnValidate(){if(onValidate)if(maxValuesSet)ChangeBar();}
    void Update(){if(!onlyOnEnable)if(maxValuesSet)ChangeBar();}
    void SetMaxValues(){
        if(valueName.Contains("cardUseTimerLeftHand")){
            int _i=int.Parse(valueName.Split("cardUseTimerLeftHand")[1]);
            if(_i<CardManager.instance.leftHand.Count){if(CardManager.instance.leftHand[_i]!=null){
                if(CardManager.instance.FindCard(CardManager.instance.leftHand[_i].idName)!=null){
                    maxValue=CardManager.instance.FindCard(CardManager.instance.leftHand[_i].idName).useTime;
                }
            }}
        }
        if(valueName.Contains("cardUseTimerMainHand")){
            int _i=int.Parse(valueName.Split("cardUseTimerMainHand")[1]);
            if(_i<CardManager.instance.hand.Count){if(CardManager.instance.hand[_i]!=null){
                if(CardManager.instance.FindCard(CardManager.instance.hand[_i].idName)!=null){
                    maxValue=CardManager.instance.FindCard(CardManager.instance.hand[_i].idName).useTime;
                }
            }}
        }
        maxValuesSet=true;
    }
    void ChangeBar(){
        if(valueName=="health"){}
        if(valueName=="energyTimer"){
            valueN=CardManager.instance.energyRegenTimer;
            maxValue=CardManager.instance.energyRegenTime;
        }
        if(valueName.Contains("cardUseTimerLeftHand")){
            int _i=int.Parse(valueName.Split("cardUseTimerLeftHand")[1]);
            if(_i<CardManager.instance.leftHand.Count){if(CardManager.instance.leftHand[_i]!=null){
                valueN=Mathf.Clamp(CardManager.instance.leftHand[_i].useTimer,0,maxValue);
            }}
        }
        if(valueName.Contains("cardUseTimerMainHand")){
            int _i=int.Parse(valueName.Split("cardUseTimerMainHand")[1]);
            if(_i<CardManager.instance.hand.Count){if(CardManager.instance.hand[_i]!=null){
                valueN=Mathf.Clamp(CardManager.instance.hand[_i].useTimer,0,maxValue);
            }}
        }

        value=valueN;
        if(reverse){value=maxValue-valueN;}
        if(value==0&&maxValue==0){maxValue=1;}//So it appears empty
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
    public void SetValueName(string _valueName){valueName=_valueName;}
}
public enum barType{
    HorizontalR,
    HorizontalL,
    VerticalU,
    VerticalD,
    Fill
}