using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class CardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{

    // void Click(){}
    public void OnPointerEnter(PointerEventData eventData){if(CardManager.instance.selectedCard==-1){transform.parent.GetComponent<CardEntity>().HoverEnter();}}
    public void OnPointerExit(PointerEventData eventData){transform.parent.GetComponent<CardEntity>().HoverExit();}
}
