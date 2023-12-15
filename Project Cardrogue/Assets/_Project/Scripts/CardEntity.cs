using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class CardEntity : MonoBehaviour{
    [Header("Config")]
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI displayNameTxt;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI descNameTxt;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI costTxt;
    [ChildGameObjectsOnly][SerializeField]Image lockSpr;
    [SerializeField]Sprite lockedSprite;
    [SerializeField]Sprite unlockedSprite;
    [SerializeField]Sprite beenlockedSprite;
    [Header("Variables")]
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
        displayNameTxt.text=card.displayName;
        descNameTxt.text=card.description;
        costTxt.text=card.cost.ToString();

        _isSetup=true;
    }
    void Update(){
        // float _step = Time.fixedDeltaTime*50f;
        float _step = Time.fixedDeltaTime*250f;
        float rotationSpeed = Time.fixedDeltaTime*50f;
        rt.localScale = Vector2.MoveTowards(rt.localScale,targetScale,_step);
        rt.anchoredPosition=Vector2.MoveTowards(rt.anchoredPosition,targetPos,_step);
        Quaternion _targetRotation = Quaternion.Euler(0f, 0f, targetRot);
        rt.localRotation = Quaternion.RotateTowards(rt.localRotation, _targetRotation, rotationSpeed);

        canvas.sortingOrder=(int)Vector2.MoveTowards(new Vector2(canvas.sortingOrder,0),new Vector2(targetZ,0),_step).x;

        if(_isSetup&&CardManager.instance.selectedCard==handId){
            targetScale=originalScale*CardManager.instance.cardSelectScaleMult;
            targetPos=new Vector2(originalPos.x,originalPos.y+CardManager.instance.cardSelectOffset);
            targetZ=CardManager.instance.handSize+10;
            targetRot=0;
        }else{
            if(CardManager.instance.hoveredCard!=handId){
                targetScale=originalScale;
                targetPos=originalPos;
                targetZ=handId;
                targetRot=originalRot;
            }
        }
        
        if(handId<CardManager.instance.hand.Count){
            if(CardManager.instance.hand[handId]!=null){
                if(CardManager.instance.hand[handId].locked&&lockSpr.sprite!=lockedSprite){
                    lockSpr.sprite=lockedSprite;
                }
                if(!CardManager.instance.hand[handId].locked){
                    if(!CardManager.instance.hand[handId].beenLocked&&lockSpr.sprite!=unlockedSprite){lockSpr.sprite=unlockedSprite;}
                    else if(CardManager.instance.hand[handId].beenLocked&&lockSpr.sprite!=beenlockedSprite){lockSpr.sprite=beenlockedSprite;}
                }
            }
        }
    }

    public void SelectCard(){
        CardManager.instance.SelectCard(handId,true);
        
        if(CardManager.instance.selectedCard!=handId){
            targetScale=originalScale;
            targetPos=originalPos;
            targetZ=handId;
            targetRot=originalRot;
        }
    }
    public void LockCard(){CardManager.instance.ToggleLockCard(handId);}
    public void HoverEnter(){
        if(_isSetup){
            targetScale=originalScale*1.2f;
            // targetPos=new Vector2(originalPos.x,originalPos.y+50f);
            // targetPos=new Vector2(originalPos.x,originalPos.y+250f);
            // targetPos=new Vector2(originalPos.x,originalPos.y+150f);
            targetPos=new Vector2(originalPos.x,originalPos.y+CardManager.instance.cardHoverOffset);
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
