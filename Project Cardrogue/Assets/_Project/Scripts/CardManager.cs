using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public class CardManager : MonoBehaviour{   public static CardManager instance;
    [Header("Config")]
    [AssetsOnly][SerializeField] GameObject cardUiPrefab;
    [SerializeField] public bool angledHandLayout=false;
    [SerializeField] RectTransform handLayoutGroup;
    public int handSize=5;
    public int energy=10;
    public int energyMax=20;
    public int locks=2;
    public int locksMax=10;
    public int lockedCardsCap=2;
    public float energyRegenTime=5f;
    public int energyRegenRate=1;
    public bool lockingBlocksUse=false;
    public bool autoRefillOnEmptyHand=true;
    public bool instaRefillWhenLeftWithUnusable=false;//If left with 1 passive
    [DisableIf("@this.instaRerollOnUse")]public bool autoRefillOnUse=false;
    [DisableIf("@this.autoRefillOnUse")]public bool instaRerollOnUse=false;
    public bool infiniteScroll=false;
    [SerializeField]public CardTypeColor[] cardTypesColors;
    [SerializeField] bool makeUpRandomCards=true;
    [HideIf("@!this.makeUpRandomCards")][SerializeField] bool _overridePresetCards=true;
    [HideIf("@!this.makeUpRandomCards")][SerializeField] int _newRandomCardsAmnt=10;
    [SerializeField] public float cardHoverOffset=150f;
    [SerializeField] public float cardHoverScaleMult=1f;
    [SerializeField] public float cardSelectOffset=160f;
    [SerializeField] public float cardSelectScaleMult=1.25f;

    [Header("Variables")]
    public List<Card> cardsList;
    public List<CardHandInfo> hand;
    [DisableInEditorMode]public int hoveredCard=-1; 
    [DisableInEditorMode]public int selectedCard=-1;
    [DisableInEditorMode]public int lastSelectedCard=-1;
    [DisableInEditorMode]public Card selectedCardRef=null;
    [DisableInEditorMode]public float energyRegenTimer;
    void Awake(){
        if(instance!=null){Destroy(gameObject);}else{instance=this;gameObject.name=gameObject.name.Split('(')[0];}
    }
    void Start(){
        if(makeUpRandomCards)RandomizeCardList();
        RerollHand();
        energyMax=100;energy=energyMax;
        locksMax=100;locks=locksMax;
        energyRegenTimer=energyRegenTime;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.R)){RerollHand();}
        if(Input.GetKeyDown(KeyCode.F)){FillHand();}
        if(Input.GetKeyDown(KeyCode.U)){UseSelectedCard();}
        if(Input.GetKeyDown(KeyCode.I)){UpdateUiWithHand();}
        if(Input.GetKeyDown(KeyCode.C)){RandomizeCardList();}
        if(Input.GetKeyDown(KeyCode.L)){ToggleLockHoveredOrSelectedCard();}
        if(Input.GetKeyDown(KeyCode.E)){energy+=10;}
        if(Input.GetKeyDown(KeyCode.K)){locks+=2;}
        
        if(Input.GetKeyDown(KeyCode.Alpha1)){SelectCard(0);}
        if(Input.GetKeyDown(KeyCode.Alpha2)){SelectCard(1);}
        if(Input.GetKeyDown(KeyCode.Alpha3)){SelectCard(2);}
        if(Input.GetKeyDown(KeyCode.Alpha4)){SelectCard(3);}
        if(Input.GetKeyDown(KeyCode.Alpha5)){SelectCard(4);}
        if(Input.mouseScrollDelta.y>0){SelectUp();}
        else if(Input.mouseScrollDelta.y<0){SelectDown();}

        if(selectedCardRef==null&&selectedCard!=-1){
        // if(selectedCardRef==null&&(selectedCard!=-1||lastSelectedCard!=selectedCard)){
            selectedCardRef=FindCard(hand[selectedCard].idName);
        }else if(selectedCard==-1){selectedCardRef=null;}
        energyMax=Mathf.Clamp(energyMax,0,9999);
        energy=Mathf.Clamp(energy,0,energyMax);
        locks=Mathf.Clamp(locks,0,locksMax);

        // foreach(CardHandInfo c in hand){if(c.locked){c.beenLocked=true;}}//This probably should be turned off if I want to implement pre-locked cards

        if(energyRegenTimer>0){energyRegenTimer-=Time.deltaTime;}
        else{
            energy+=energyRegenRate;
            energyRegenTimer=energyRegenTime;
        }

        if(instaRefillWhenLeftWithUnusable){
            if(hand.Count==1){
                if(FindCard(hand[0].idName).cardType==cardType.passive){
                    FillHand();
                }
            }
        }
    }

    [Button("RandomizeCardList")]
    public void RandomizeCardList(){
        if(_overridePresetCards)cardsList=new List<Card>(0);
        int cardsLitCountBefore=cardsList.Count;
        for(int i=cardsLitCountBefore;i<cardsLitCountBefore+_newRandomCardsAmnt;i++){
            cardsList.Add(new Card {
                idName = "card" + (i+1),
                displayName = "card " + (i+1),
                description = "card " + (i+1) + " description.",
                cardType = (cardType)Random.Range(0, 3+1),
                cost = Random.Range(0, 4+1),
                useRange = Random.Range(1, 5+1),
                useTime = Random.Range(1, 5+1)
            });
        }
        foreach(Card c in cardsList){
            if(c.cardType==cardType.passive){c.cost=1;c.useRange=0;c.useTime=Random.Range(0, 30+1);}//it could be interesting if a passive card would take energy per use time
            if(c.cardType==cardType.instaUse){c.useRange=0;}//c.useTime=0;}
        }
    }

    [Button("FillHand")]
    public void FillHand(){
        for(int i=hand.Count;i<handSize;i++){
            int _randCardId=Random.Range(0, cardsList.Count);
            AddCleanHandInfo(cardsList[_randCardId].idName);
        }
        UpdateUiWithHand();
    }
    [Button("RerollHand")]
    public void RerollHand(){
        List<CardHandInfo> lockedCards=new List<CardHandInfo>();
        if(hand!=null){if(hand.Count>0){
            for(int i=0;i<hand.Count;i++){
                if(FindCard(hand[i].idName)!=null){
                    if(hand[i].locked){lockedCards.Add(CloneHandInfo(hand[i]));}
                }
            }
        }}
        hand=new List<CardHandInfo>(0);
        for(int i=0;i<lockedCards.Count;i++){//Readd locked cards
            hand.Add(CloneHandInfo(lockedCards[i]));
        }
        for(int i=hand.Count-1;i<handSize-1;i++){
            int _randCardId=Random.Range(0, cardsList.Count);
            AddCleanHandInfo(cardsList[_randCardId].idName);
        }
        selectedCard=-1;selectedCardRef=null;
        UpdateUiWithHand();
    }
    float minMaxCardRotationAngleNormal = 5f;
    float minMaxCardRotationAngle = 40f;
    float verticalOffsetFactor = 10f;
    float verticalOffsetFactorAngled = 40f;
    [Button("UpdateUiWithHand")]
    public void UpdateUiWithHand(){
        for(int i=handLayoutGroup.childCount-1;i>=0;i--){
            Destroy(handLayoutGroup.GetChild(i).gameObject);
        }

        for(int i=0;i<hand.Count;i++){
            CardHandInfo card = hand[i];
            GameObject _cardUi=Instantiate(cardUiPrefab,handLayoutGroup);
            _cardUi.name="Card "+(i+1);
            CardEntity _cardEntity=_cardUi.GetComponent<CardEntity>();

            float verticalOffset=0;
            if(angledHandLayout){
                float normalizedPos = (float)i / (Mathf.Clamp(hand.Count,2,99) - 1);
                verticalOffset = verticalOffsetFactorAngled * Mathf.Abs(normalizedPos - 0.5f);
                if(hand.Count<2){verticalOffset=0;}
            }else{
                verticalOffset=verticalOffsetFactor*i;
            }

            _cardEntity.transform.GetChild(0).GetComponent<RectTransform>().localPosition=new Vector3(0,-verticalOffset,0);

            float _angle=0;
            if(angledHandLayout){
                _angle=Mathf.Lerp(minMaxCardRotationAngle, -minMaxCardRotationAngle, (float)i / (Mathf.Clamp(hand.Count,2,99) - 1));
                if(hand.Count<2){_angle=0;}
            }else{
                _angle=Mathf.Lerp(minMaxCardRotationAngleNormal, -minMaxCardRotationAngleNormal, (float)i / (Mathf.Clamp(hand.Count,2,99) - 1));
                if(hand.Count<2){_angle=0;}
            }
            _cardEntity.SetProperties(i,card.idName,_angle);
            // _cardEntity.originalRot=_angle;
        }
    }

    public void SelectCard(int cardId,bool forceUnselect=false,bool disallowInstaUse=false){
        if(cardId>=0&&cardId<hand.Count){
            if(FindCard(hand[cardId].idName).cardType==cardType.instaUse&&!disallowInstaUse){lastSelectedCard=selectedCard;UseCard(cardId);selectedCard=-1;selectedCardRef=null;return;}
            if(FindCard(hand[cardId].idName).cardType!=cardType.passive){
                lastSelectedCard=selectedCard;
                if(selectedCard==cardId){selectedCard=-1;return;}
                if(selectedCard!=cardId&&selectedCard!=-1&&forceUnselect){selectedCard=-1;return;}//For unselecting by pressing a different card
                selectedCard=cardId;
                selectedCardRef=FindCard(hand[selectedCard].idName);
            }else{selectedCard=-1;}
        }else{Debug.LogWarning(cardId+" is an incorrect hand card id!");}
    }
    public void SelectUp(){
        for(int i=selectedCard+1; i<hand.Count; i++){
            if(FindCard(hand[i].idName).cardType != cardType.passive){
                SelectCard(i, disallowInstaUse:true);
                return;
            }
        }

        if(infiniteScroll){
        for(int i=0; i<hand.Count; i++){
            if(FindCard(hand[i].idName).cardType != cardType.passive){
                SelectCard(i, disallowInstaUse: true);
                return;
            }
        }
    }
    }
    public void SelectDown(){
        if(selectedCard==-1&&hand.Count>1){
            if(FindCard(hand[hand.Count-1].idName).cardType != cardType.passive){
                SelectCard(hand.Count-1, disallowInstaUse:true);
            }//else{selectedCard=hand.Count-1;}
        }

        for(int i=selectedCard-1; i>=0; i--){
            if(FindCard(hand[i].idName).cardType != cardType.passive){
                SelectCard(i, disallowInstaUse:true);
                return;
            }
        }

        if(infiniteScroll){
            for(int i=hand.Count-1; i>=0; i--){
                if(FindCard(hand[i].idName).cardType != cardType.passive){
                    SelectCard(i, disallowInstaUse: true);
                    return;
                }
            }
        }
    }

    [Button("UseSelectedCard")]
    public void UseSelectedCard(){if(selectedCard!=-1)UseCard(selectedCard);}
    public void UseCard(int cardId){
        if(cardId>=0&&cardId<hand.Count){
            Card card=FindCard(hand[cardId].idName);
            if(energy>=card.cost&&(!lockingBlocksUse||(lockingBlocksUse&&!hand[cardId].locked))){
                if(card.cardType!=cardType.passive){
                    RemoveCardFromHand(cardId);
                    energy-=card.cost;
                    if(autoRefillOnUse){FillHand();}
                    if(instaRerollOnUse){RerollHand();}
                    else{UpdateUiWithHand();}
                }
            }else{
                if(energy<card.cost){
                    Debug.Log("You broke");
                }
                if(lockingBlocksUse&&hand[cardId].locked){
                    Debug.Log("Its locked");
                }
            }
            selectedCard=-1;
            selectedCardRef=null;
            if(hand.Count<=0&&autoRefillOnEmptyHand){
                RerollHand();
            }
            return;
        }else{Debug.LogWarning(cardId+" is an incorrect hand card id!");}
    }
    public void LockSelectedCard(){LockCard(selectedCard);}
    public void LockCard(int cardId){
        if(cardId>=0&&cardId<hand.Count){
            if(hand.FindAll(x=>x.locked).Count<lockedCardsCap||lockedCardsCap==-1){
                if(locks>0||hand[cardId].beenLocked){
                    if(!hand[cardId].beenLocked)locks-=1;

                    hand[cardId].locked=true;
                    hand[cardId].beenLocked=true;
                }else if(locks<=0){
                    Debug.Log("No locks left.");
                }
            }else if(hand.FindAll(x=>x.locked).Count>=lockedCardsCap&&lockedCardsCap!=-1){
                Debug.Log("Locked cards capped.");
            }
        }else{Debug.LogWarning(cardId+" is an incorrect hand card id!");}
    }
    public void UnlockSelectedCard(){UnlockCard(selectedCard);}
    public void UnlockCard(int cardId){
        if(cardId>=0&&cardId<hand.Count){
            if(locks>0||hand[cardId].beenLocked){
                if(!hand[cardId].beenLocked)locks-=1;//Lets say if it was not locked by the player or smth

                hand[cardId].locked=false;
            }
        }else{Debug.LogWarning(cardId+" is an incorrect hand card id!");}
    }
    public void ToggleLockHoveredCard(){ToggleLockCard(hoveredCard);}
    public void ToggleLockSelectedCard(){ToggleLockCard(selectedCard);}
    public void ToggleLockHoveredOrSelectedCard(){
        if(hoveredCard!=-1)ToggleLockCard(hoveredCard);
        if(selectedCard!=-1)ToggleLockCard(selectedCard);
    }
    public void ToggleLockCard(int cardId){
        if(cardId>=0&&cardId<hand.Count){
            if(!hand[cardId].locked&&hand.FindAll(x=>x.locked).Count>=lockedCardsCap&&lockedCardsCap!=-1){
                Debug.Log("Locked cards capped.");
            }
            if(hand[cardId].locked||(!hand[cardId].locked&&hand.FindAll(x=>x.locked).Count<lockedCardsCap||lockedCardsCap==-1)){
                if(locks>0||hand[cardId].beenLocked){
                    if(!hand[cardId].beenLocked){locks-=1;}

                    hand[cardId].locked=!hand[cardId].locked;
                    if(hand[cardId].locked){hand[cardId].beenLocked=true;}
                    return;
                }else if(locks<=0){
                    Debug.Log("No locks left.");
                }
            }
            
        }else{Debug.LogWarning(cardId+" is an incorrect hand card id!");}
    }

    public void RemoveCardFromHand(int cardId){hand.RemoveAt(cardId);}
    void AddCleanHandInfo(string cardIdName){
        hand.Add(new CardHandInfo(){
            idName=cardIdName,
            useTimer=-1,
            inactive=false,
            locked=false,
        });
    }

    
    CardHandInfo CloneHandInfo(CardHandInfo originalCardHandInfo){
        string json = JsonUtility.ToJson(originalCardHandInfo);
        CardHandInfo clonedCardHandInfo = JsonUtility.FromJson<CardHandInfo>(json);
        return clonedCardHandInfo;
    }
    Card CloneCard(Card originalCard){
        string json = JsonUtility.ToJson(originalCard);
        Card clonedCard = JsonUtility.FromJson<Card>(json);
        return clonedCard;
    }
    public Card FindCard(string idName){
        return cardsList.Find(x=>x.idName==idName);
    }
}

[System.Serializable]
public class Card{
    public string idName;
    public string displayName;
    [TextArea]public string description;
    public cardType cardType;
    public int cost;
    private bool IsNotPassive() => cardType != cardType.passive;
    private bool IsNotPassiveOrInstaUse() => cardType != cardType.passive && cardType != cardType.instaUse;
    [ShowIf("IsNotPassiveOrInstaUse")]public float useRange=3f;
    [ShowIf("IsNotPassive")]public float useTime=2f;
}
[System.Serializable]
public class CardHandInfo{
    public string idName;
    public float useTimer=-1;
    public bool inactive;
    public bool locked;
    public bool beenLocked;
}
public enum cardType{instaUse,ability,structure,passive}
[System.Serializable]
public class CardTypeColor{
    [SerializeField] public cardType cardType;
    [SerializeField] public Color color=Color.white;
}