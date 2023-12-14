using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class CardEntity : MonoBehaviour{//, IPointerEnterHandler, IPointerExitHandler{
    [ReadOnly]public int handId;
    [ReadOnly]public string cardIdName;
    Vector2 originalScale;
    Vector2 targetScale;
    Vector2 originalPos;
    Vector2 targetPos;
    int targetZ;
    [HideInEditorMode]public float originalRot;
    float targetRot;
    bool _isSetup;
    RectTransform rt;
    Image img;
    Canvas canvas;

    void Start(){
        // rt=GetComponent<RectTransform>();
        rt=transform.GetChild(0).GetComponent<RectTransform>();
        originalScale=rt.localScale;
        targetScale=originalScale;
        originalPos=rt.anchoredPosition;
        targetPos=originalPos;
        // targetRot=originalRot;
        img=GetComponent<Image>();
        canvas=GetComponent<Canvas>();
    }
    public void SetProperties(int _id, string _idName="",float angle=0){
        handId=_id;
        if(_idName!=""){cardIdName=_idName;}
        Card card=CardManager.instance.FindCard(this.cardIdName);

        if(rt==null){rt=transform.GetChild(0).GetComponent<RectTransform>();}
        
        originalRot=angle;
        // rt.localEulerAngles=new Vector3(rt.localEulerAngles.x,rt.localEulerAngles.y,originalRot);
        rt.localRotation = Quaternion.Euler(0f, 0f, targetRot);
        targetRot=originalRot;

        if(canvas==null){canvas=GetComponent<Canvas>();}
        targetZ=handId;
        canvas.sortingOrder=targetZ;

        transform.GetChild(0).GetComponent<Image>().color=CardManager.instance.cardTypesColors[(int)card.cardType].color;
        transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text=card.displayName;
        transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text=card.description;
        transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text=card.cost.ToString();

        _isSetup=true;
    }
    void Update(){
        float _step = Time.fixedDeltaTime*50;
        float rotationSpeed = Time.fixedDeltaTime*50;
        // rt.localScale=Vector2.MoveTowards(rt.localScale,targetScale,_step);
        rt.localScale = Vector2.MoveTowards(rt.localScale,targetScale,_step);
        // rt.anchoredPosition=Vector2.MoveTowards(rt.anchoredPosition,targetPos,_step);
        // rt.localEulerAngles=Vector3.MoveTowards(new Vector3(rt.localEulerAngles.x,rt.localEulerAngles.y,rt.localEulerAngles.z),new Vector3(rt.localEulerAngles.x,rt.localEulerAngles.y,targetRot),_step);
        Quaternion _targetRotation = Quaternion.Euler(0f, 0f, targetRot);
        rt.localRotation = Quaternion.RotateTowards(rt.localRotation, _targetRotation, rotationSpeed);

        // while(transform.GetSiblingIndex()<transform.parent.childCount-1){transform.SetSiblingIndex((int)Vector2.MoveTowards(new Vector2(transform.GetSiblingIndex(),0),new Vector2(targetZ,0),_step).x);}
        // transform.SetSiblingIndex((int)Vector2.MoveTowards(new Vector2(transform.GetSiblingIndex(),0),new Vector2(targetZ,0),_step).x);
        canvas.sortingOrder=(int)Vector2.MoveTowards(new Vector2(canvas.sortingOrder,0),new Vector2(targetZ,0),_step).x;

        if(_isSetup&&CardManager.instance.selectedCard==handId){
            targetScale=originalScale*1.25f;
            targetPos=new Vector2(originalPos.x,originalPos.y*1.8f);
            targetZ=CardManager.instance.handSize+10;
            targetRot=0;
        }
    }

    public void SelectCard(){
        if(CardManager.instance.FindCard(cardIdName).cardType!=cardType.passive){
            CardManager.instance.SelectCard(handId);
        }
        if(CardManager.instance.FindCard(cardIdName).cardType==cardType.instaUse){
            CardManager.instance.UseCard(handId);
        }
        if(CardManager.instance.selectedCard!=handId){
                targetScale=originalScale;
                targetPos=originalPos;
                targetZ=handId;
                targetRot=originalRot;
            }
    }
    // public void OnPointerEnter(PointerEventData eventData){
    //     if(_isSetup){
    //         targetScale=originalScale*1.2f;
    //         targetPos=new Vector2(originalPos.x,originalPos.y*1.2f);
    //         targetZ=transform.parent.childCount-1;
    //     }
    // }
    // public void OnPointerExit(PointerEventData eventData){
    //     if(_isSetup){
    //         targetScale=originalScale;
    //         targetPos=originalPos;
    //         targetZ=handId;
    //     }
    // }
    public void HoverEnter(){
        if(_isSetup){
            targetScale=originalScale*1.2f;
            targetPos=new Vector2(originalPos.x,originalPos.y*1.2f);
            targetZ=CardManager.instance.handSize+5;
            targetRot=0;
        }
    }
    public void HoverExit(){
        if(_isSetup&&CardManager.instance.selectedCard!=this.handId){
            targetScale=originalScale;
            targetPos=originalPos;
            targetZ=handId;
            targetRot=originalRot;
        }
    }
}
