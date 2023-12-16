using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;

public class PopupUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
    [DisableIf("@this.defaultHidden")][SerializeField] bool hoverable=false;
    [DisableIf("@this.hoverable")][SerializeField] bool defaultHidden=true;
    [SerializeField] float shownPos=250f;
    [SerializeField] float hiddenPos=-20f;
    Vector2 targetPos;
    RectTransform rt;
    void Start(){
        rt=GetComponent<RectTransform>();
        targetPos=new Vector2(0,hiddenPos);
        rt.anchoredPosition=targetPos;
    }
    void Update(){
        float _step=Time.fixedDeltaTime*100;
        rt.anchoredPosition=Vector2.MoveTowards(rt.anchoredPosition,targetPos,_step);

        if(!hoverable){
            if(defaultHidden){if(targetPos.y!=hiddenPos)targetPos=new Vector2(0,hiddenPos);}
            else{if(targetPos.y!=shownPos)targetPos=new Vector2(0,shownPos);}
        }
    }
    public void Show(){targetPos=new Vector2(0,shownPos);}
    public void Hide(){targetPos=new Vector2(0,hiddenPos);}
    public void OnPointerEnter(PointerEventData eventData){if(hoverable)Show();}
    public void OnPointerExit(PointerEventData eventData){if(hoverable)Hide();}
}