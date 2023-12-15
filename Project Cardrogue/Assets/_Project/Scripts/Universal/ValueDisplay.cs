using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class ValueDisplay : MonoBehaviour{
    [SerializeField] public string value="score";
    [DisableInPlayMode][SerializeField] bool onlyOnEnable=false;
    [HideInPlayMode][SerializeField] bool onValidate=false;
    TextMeshProUGUI txt;
    TMP_InputField tmpInput;
    void Start(){
        if(GetComponent<TextMeshProUGUI>()!=null)txt=GetComponent<TextMeshProUGUI>();
        if(GetComponent<TMP_InputField>()!=null)tmpInput=GetComponent<TMP_InputField>();
        if(onlyOnEnable){ChangeText();}
    }
    void OnEnable(){if(onlyOnEnable){ChangeText();}}
    void OnValidate(){if(onValidate){ChangeText();}}
    void Update(){if(!onlyOnEnable){ChangeText();}}


    void ChangeText(){      string _txt="";
        if(value=="energy"){_txt=CardManager.instance.energy.ToString();}
        else if(value=="locks"){_txt=CardManager.instance.locks.ToString();}
        
        if(txt!=null)txt.text=_txt;
        // else{if(tmpInput!=null){if(UIInputSystem.instance!=null)if(UIInputSystem.instance.currentSelected!=tmpInput.gameObject){tmpInput.text=_txt;}
        // foreach(TextMeshProUGUI t in GetComponentsInChildren<TextMeshProUGUI>()){t.text=_txt;}}}
    }
}
