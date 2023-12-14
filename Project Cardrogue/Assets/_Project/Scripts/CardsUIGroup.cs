using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class CardsUIGroup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
    [SerializeField] float defaultPos=250f;
    [SerializeField] float hiddenPos=-20f;
    Vector2 targetPos;
    // [SerializeField] float defaultPadding=45f;
    // [SerializeField] float hiddenPadding=-200f;
    // float targetPadding;
    // HorizontalLayoutGroup layoutGroup;
    RectTransform rt;
    void Start(){
        rt=GetComponent<RectTransform>();
        targetPos=new Vector2(0,hiddenPos);
        // layoutGroup=GetComponent<HorizontalLayoutGroup>();
        // defaultPadding=layoutGroup.padding.bottom;
        // targetPadding=hiddenPadding;
    }
    void Update(){
        float _step=Time.fixedDeltaTime*100;
        rt.anchoredPosition=Vector2.MoveTowards(rt.anchoredPosition,targetPos,_step);
        // layoutGroup.padding.bottom=(int)Mathf.Lerp(layoutGroup.padding.bottom,targetPadding,_step);
        // if(layoutGroup.padding.bottom!=targetPadding){LayoutRebuilder.ForceRebuildLayoutImmediate(rt);}

    }
    public void OnPointerEnter(PointerEventData eventData){targetPos=new Vector2(0,defaultPos);}//targetPadding=defaultPadding;}
    public void OnPointerExit(PointerEventData eventData){targetPos=new Vector2(0,hiddenPos);}//targetPadding=hiddenPadding;}
}
