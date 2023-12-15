using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class CardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
    CardEntity parentCardEnt;
    void Start(){
        parentCardEnt=transform.parent.GetComponent<CardEntity>();
    }
    public void OnPointerEnter(PointerEventData eventData){
        if(CardManager.instance.selectedCard==-1&&CardManager.instance.hoveredCard!=parentCardEnt.handId){
            CardManager.instance.hoveredCard=parentCardEnt.handId;
            parentCardEnt.HoverEnter();
        }
    }
    public void OnPointerExit(PointerEventData eventData){
        parentCardEnt.HoverExit();
        if(CardManager.instance.hoveredCard==parentCardEnt.handId&&CardManager.instance.hoveredCard!=-1){
            CardManager.instance.hoveredCard=-1;
        }
    }
}
