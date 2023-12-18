using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;

public class LeftHandList : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
    public void OnPointerEnter(PointerEventData eventData){CardManager.instance.leftHandIsHovered=true;}
    public void OnPointerExit(PointerEventData eventData){}//CardManager.instance.leftHandIsHovered=false;CardManager.instance.leftHandHoveredId=-1;}
}
