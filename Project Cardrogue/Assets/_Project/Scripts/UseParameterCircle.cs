using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UseParameterCircle : MonoBehaviour, IPointerDownHandler{//, IPointerEnterHandler, IPointerExitHandler{
    public void OnPointerDown(PointerEventData eventData){
        Vector2 worldEventPos=Camera.main.ScreenToWorldPoint(eventData.position);
        Debug.Log("Clicked on " + gameObject.name + " at(screen): "+eventData.position+" | world: "+worldEventPos);
        if(CardManager.instance.selectedCard!=-1&&CardManager.instance.selectedCardRef!=null){
            CardManager.instance.UseSelectedCard();
        }
    }
}
