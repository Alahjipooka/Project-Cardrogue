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
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI idTxt;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI displayNameTxt;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI descNameTxt;
    [ChildGameObjectsOnly][SerializeField]GameObject costParent;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI costTxt;
    [ChildGameObjectsOnly][SerializeField]GameObject useTimeParent;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI useTimeTxt;
    [ChildGameObjectsOnly][SerializeField]GameObject useRangeParent;
    [ChildGameObjectsOnly][SerializeField]TextMeshProUGUI useRangeTxt;
    [ChildGameObjectsOnly][SerializeField]BarValue useTimerOverlay;
    [ChildGameObjectsOnly][SerializeField]GameObject dismissParent;
    [ChildGameObjectsOnly][SerializeField]GameObject lockParent;
    [ChildGameObjectsOnly][SerializeField]Image lockSpr;
    [SerializeField]Sprite lockedSprite;
    [SerializeField]Sprite unlockedSprite;
    [SerializeField]Sprite beenlockedSprite;
    [Header("Variables")]
    [ReadOnly]public bool isLeftHand;
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
    public void SetProperties(int _id, bool _isLeftHand=false, string _idName="",float angle=0){
        handId=_id;
        isLeftHand=_isLeftHand;
        if(_idName!=""){cardIdName=_idName;}
        Card card=CardManager.instance.FindCard(this.cardIdName);
        CardHandInfo cardHandInfo;
        if(!isLeftHand){cardHandInfo=CardManager.instance.hand[handId];}
        else{cardHandInfo=CardManager.instance.leftHand[handId];}

        if(rt==null){rt=transform.GetChild(0).GetComponent<RectTransform>();}
        
        originalRot=angle;
        // rt.localEulerAngles=new Vector3(rt.localEulerAngles.x,rt.localEulerAngles.y,originalRot);
        rt.localRotation = Quaternion.Euler(0f, 0f, targetRot);
        targetRot=originalRot;

        if(canvas==null){canvas=GetComponent<Canvas>();}
        targetZ=handId;
        canvas.sortingOrder=targetZ;

        transform.GetChild(0).GetComponent<Image>().color=CardManager.instance.cardTypesColors[(int)card.cardType].color;
        idTxt.text=(handId+1).ToString();
        displayNameTxt.text=card.displayName;
        descNameTxt.text=card.description;
        costTxt.text=cardHandInfo.costCurrent.ToString();
        useTimeTxt.text=card.useTime.ToString();
        useRangeTxt.text=card.useRange.ToString();
        if(isLeftHand){useTimerOverlay.SetValueName("cardUseTimerLeftHand"+handId);}
        else{useTimerOverlay.SetValueName("cardUseTimerMainHand"+handId);}
        if(card.useTime<=0){useTimeParent.SetActive(false);}
        if(card.useRange<=0){useRangeParent.SetActive(false);}

        if(isLeftHand){
            if(card.loopingTimes>0){
                idTxt.text=cardHandInfo.loopedTimes+" / "+card.loopingTimes;
            }else{idTxt.text="";}
            Destroy(GetComponentInChildren<CardButton>().GetComponent<Button>());
            
            if(CardManager.instance.FindCard(this.cardIdName).loopingTimes==0){costParent.SetActive(false);}
            lockParent.SetActive(false);
            if(!card.dismissable)dismissParent.SetActive(false);
        }else{
            dismissParent.SetActive(false);
        }
        
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

        if(!isLeftHand){
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
        }else{//If in the using list
            if(_isSetup&&CardManager.instance.leftHandHoveredId==handId){
                targetZ=CardManager.instance.leftHand.Count+5;
                targetRot=0;
            }else{
                if(CardManager.instance.leftHandHoveredId!=handId){
                    targetZ=handId;
                    // targetZ=CardManager.instance.leftHand.Count-(handId+1);//Reverse
                    targetRot=originalRot;
                }
            }
        }
    }

    public void SelectCard(){
        CardManager.instance.SelectCard(handId,forceUnselect:true,allowInstaUse:true);
        
        if(CardManager.instance.selectedCard!=handId){
            targetScale=originalScale;
            targetPos=originalPos;
            targetZ=handId;
            targetRot=originalRot;
        }
    }
    public void LockCard(){CardManager.instance.ToggleLockCard(handId);}
    public void DismissCard(){
        if(!isLeftHand){CardManager.instance.RemoveCardFromLeftHand(handId);}
        else{CardManager.instance.RemoveCardFromLeftHand(handId);}
    }

    public void HoverEnter(){
        if(_isSetup){
            if(!isLeftHand){
                targetScale=originalScale*CardManager.instance.cardHoverScaleMult;
                targetPos=new Vector2(originalPos.x,originalPos.y+CardManager.instance.cardHoverOffset);
                targetZ=CardManager.instance.handSize+5;
                targetRot=0;
            }else{
                targetZ=CardManager.instance.leftHand.Count+5;
                targetRot=0;
            }
        }
    }
    public void HoverExit(){
        if(_isSetup){
            if(!isLeftHand&&CardManager.instance.selectedCard!=this.handId){
                targetScale=originalScale;
                targetPos=originalPos;
                targetZ=handId;
                targetRot=originalRot;
            }else if(isLeftHand){
                targetZ=handId;
                // targetZ=CardManager.instance.leftHand.Count-(handId+1);//Reverse
                targetRot=originalRot;
            }
        }
    }
}
