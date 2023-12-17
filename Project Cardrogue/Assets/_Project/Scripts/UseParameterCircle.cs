using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UseParameterCircle : MonoBehaviour, IPointerDownHandler{//, IPointerEnterHandler, IPointerExitHandler{
    [SerializeField] bool onlyAllowInstaAndStatus=false;
    public void OnPointerDown(PointerEventData eventData){
        Vector2 worldEventPos=Camera.main.ScreenToWorldPoint(eventData.position);
        Debug.Log("Clicked on " + gameObject.name + " at(screen): "+eventData.position+" | world: "+worldEventPos);
        if(CardManager.instance.selectedCard!=-1&&CardManager.instance.selectedCardRef!=null){
            if(!onlyAllowInstaAndStatus||(onlyAllowInstaAndStatus&&CardManager.instance.selectedCardRef.cardType==cardType.status||CardManager.instance.selectedCardRef.cardType==cardType.instaUse)){
                if(onlyAllowInstaAndStatus&&CardManager.instance.selectedCardRef.cardType==cardType.status||CardManager.instance.selectedCardRef.cardType==cardType.instaUse){worldEventPos=Player.instance.transform.position;}
                CardManager.instance.UseSelectedCardAtPos(worldEventPos);
            }
        }
    }
}
