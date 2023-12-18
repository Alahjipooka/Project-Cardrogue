using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public class CardManager : MonoBehaviour{   public static CardManager instance;
    [Header("UI Config")]
    [AssetsOnly][SerializeField] GameObject cardUiPrefab;
    [SerializeField] public bool angledHandLayout=false;
    [DisableIf("angledHandLayout")][SerializeField] public bool automaticallyAngleWhenAbove=true;
    [SerializeField] bool clickingOnSameCardUnselects=true;
    [SerializeField] bool clickingOnDifferentCardUnselects=true;
    [DisableIf("angledHandLayout")][ShowIf("automaticallyAngleWhenAbove")][SerializeField] public int automaticallyAngleWhenAboveNumber=6;
    [SerializeField] RectTransform handLayoutGroup;
    [SerializeField] RectTransform leftHandLayoutGroup;
    public bool infiniteScroll=false;
    [SerializeField] public float cardHoverOffset=150f;
    [SerializeField] public float cardHoverScaleMult=1.2f;
    [SerializeField] public float cardSelectOffset=160f;
    [SerializeField] public float cardSelectScaleMult=1.25f;
    [SerializeField]public CardTypeColor[] cardTypesColors;
    [Header("Config")]
    public int handSize=5;
    public int energy=10;
    public int energyMax=20;
    public int locks=2;
    public int locksMax=10;
    public int lockedCardsCap=2;
    public bool handCapsAtMaxPlusLocked=false;
    public bool autoMoveStatusToLeftHand=true;
    [HideIf("autoMoveStatusToLeftHand")]public bool autoMoveMainHandStatusToLeftHandOnReroll=false;
    [EnableIf("@this.autoMoveMainHandStatusToLeftHandOnReroll || this.autoMoveMainHandStatusToLeftHandOnReroll")]public bool keepStatusCardsWhenLeftHandFullOnReroll=false;
    public int leftHandCap=-1;
    public float energyRegenTime=5f;
    public int energyRegenRate=1;
    public bool lockingBlocksUse=false;
    public bool unselectCardsAfterUsing=false;
    public bool selectLastCardAfterUsing=false;
    public bool autoDrawOn=true;
    public float autoDrawTime=5;
    public bool autoRefillOnEmptyHand=true;
    public bool instaRefillWhenLeftWithUnusable=false;//If left with 1 status card in mainHand
    [DisableIf("@this.instaRerollOnUse")]public bool autoRefillOnUse=false;
    [DisableIf("@this.autoRefillOnUse")]public bool instaRerollOnUse=false;
    [SerializeField] bool makeUpRandomCards=true;
    [HideIf("@!this.makeUpRandomCards")][SerializeField] bool _overridePresetCards=false;
    [HideIf("@!this.makeUpRandomCards")][SerializeField] int _newRandomCardsAmnt=10;

    [Header("Variables")]
    public List<Card> cardsList;
    public List<CardHandInfo> hand;
    public List<CardHandInfo> leftHand;
    [DisableInEditorMode]public int hoveredCard=-1; 
    [DisableInEditorMode]public int selectedCard=-1;
    [DisableInEditorMode]public int lastSelectedCard=-1;
    [DisableInEditorMode]public Card selectedCardRef=null;
    [DisableInEditorMode]public bool leftHandIsHovered;
    [DisableInEditorMode]public int leftHandHoveredId=-1;
    [DisableInEditorMode]public float energyRegenTimer;
    [DisableInEditorMode]public float autoDrawTimer=-1;
    void Awake(){if(instance!=null){Destroy(gameObject);}else{instance=this;gameObject.name=gameObject.name.Split('(')[0];}}
    void Start(){
        if(makeUpRandomCards)RandomizeCardList();
        RerollHand();
        energyMax=100;energy=energyMax;
        locksMax=100;locks=locksMax;
        energyRegenTimer=energyRegenTime;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.R)){RerollHand();}
        if(Input.GetKeyDown(KeyCode.T)){DrawCard();}
        if(Input.GetKeyDown(KeyCode.F)){FillHand();}
        if(Input.GetKeyDown(KeyCode.U)){UseSelectedCard();}
        if(Input.GetKeyDown(KeyCode.I)){UpdateUI();}
        if(Input.GetKeyDown(KeyCode.O)){RandomizeCardList();}
        if(Input.GetKeyDown(KeyCode.C)){CleanUsingCardList();}
        if(Input.GetKeyDown(KeyCode.L)){ToggleLockHoveredOrSelectedCard();}
        if(Input.GetKeyDown(KeyCode.E)){energy+=10;}
        if(Input.GetKeyDown(KeyCode.K)){locks+=2;}
        for(int i=0; i<=8; i++){
            if(Input.GetKeyDown(KeyCode.Alpha1 + i)){
                SelectCard(i);
            }
        }
        if(Input.GetKeyDown(KeyCode.Alpha0)){SelectCard(9);}
        if(Input.mouseScrollDelta.y>0||Input.GetKeyDown(KeyCode.Equals)){
            if(!leftHandIsHovered){SelectUp();}
            else{SelectLeftHandUp();}
        }
        else if(Input.mouseScrollDelta.y<0||Input.GetKeyDown(KeyCode.Minus)){
            if(!leftHandIsHovered){SelectDown();}
            else{SelectLeftHandDown();}
        }

        ///Clamp values, count timers etc
        if(selectedCardRef==null&&selectedCard!=-1){
            if(selectedCard<hand.Count){
                selectedCardRef=FindCard(hand[selectedCard].idName);
            }else{Debug.LogWarning("Cant find and set reference to a selected card by id in hand: "+selectedCard);}
        }else if(selectedCard<0){selectedCardRef=null;}
        // if(selectedCard!=-1){leftHandIsHovered=false;} //Disable accidental scrolling of leftHand when a mainHand card is already selected?
        if(selectedCard>=hand.Count){selectedCard=hand.Count-1;}
        if(hand.Count<=0 || selectedCard<-1){selectedCard=-1;}
        if(!leftHandIsHovered || leftHand.Count<=0){leftHandHoveredId=-1;}
        if(hoveredCard!=-1){leftHandIsHovered=false;}
        if(selectedCard>=0&&selectedCard<hand.Count){hoveredCard=-1;}//So it doesnt glitch out with 2 cards highlighted

        energyMax=Mathf.Clamp(energyMax,0,9999);
        energy=Mathf.Clamp(energy,0,energyMax);
        locks=Mathf.Clamp(locks,0,locksMax);

        if(energyRegenTimer>0){energyRegenTimer-=Time.deltaTime;}
        else{
            AddEnergy(energyRegenRate);
            energyRegenTimer=energyRegenTime;
        }

        if(autoDrawOn){
            if(hand.Count<handSize){
                if(autoDrawTimer>0){autoDrawTimer-=Time.deltaTime;}
                else{autoDrawTimer=autoDrawTime;DrawCard();}
            }else{autoDrawTimer=autoDrawTime;}
        }


        /// Tick down useTimers and UseCards
        for(int i=0;i<hand.Count;i++){
            Card _card=FindCard(hand[i].idName);
            if(_card.tickInMainHand&&!hand[i].inactive){
                if(hand[i].useTimer>0){hand[i].useTimer-=Time.deltaTime;}
                else{if(hand[i].useTimer!=-1){
                    energy-=hand[i].costCurrent;
                    CardFunctionsManager.instance.CardFunction(false,hand[i].idName,Vector2.zero);

                    hand[i].useTimer=_card.useTime;
                    if(_card.loopingTimes!=-1)hand[i].loopedTimes+=1;
                    UpdateHandUI();

                    if(_card.loopingTimes!=-1&&hand[i].loopedTimes>=_card.loopingTimes){
                        RemoveCardFromHand(i);
                    }
                }}
            }
        }
        for(int i=0;i<leftHand.Count;i++){
            if(!leftHand[i].inactive){
                Card _card=FindCard(leftHand[i].idName);
                if(leftHand[i].useTimer>0){leftHand[i].useTimer-=Time.deltaTime;}
                else{if(leftHand[i].useTimer!=-1){
                    energy-=leftHand[i].costCurrent;
                    CardFunctionsManager.instance.CardFunction(false,leftHand[i].idName,Vector2.zero);

                    leftHand[i].useTimer=_card.useTime;
                    if(_card.loopingTimes!=-1)leftHand[i].loopedTimes+=1;
                    UpdateLeftHandUI();

                    if(_card.loopingTimes!=-1&&leftHand[i].loopedTimes>=_card.loopingTimes){
                        RemoveCardFromLeftHand(i);
                    }
                }}
            }
        }

        //Not sure if I should leave this or refactor it into unplayable
        if(instaRefillWhenLeftWithUnusable){
            if(hand.Count==1){
                if(FindCard(hand[0].idName).cardType==cardType.status){
                // if(hand[0].isUnplayable){
                    FillHand();
                }
            }
        }

        /// Update Hand Layout when needed
        if(automaticallyAngleWhenAbove){
            if(hand.Count>automaticallyAngleWhenAboveNumber){
                if(!angledHandLayout){
                    angledHandLayout=true;
                    UpdateUI();
                }
            }else{
                if(angledHandLayout){
                    angledHandLayout=false;
                    UpdateUI();
                }
            }
        }
    }

    [Button("RandomizeCardList")]
    public void RandomizeCardList(){
        if(_overridePresetCards)cardsList=new List<Card>(0);
        int cardsLitCountBefore=cardsList.Count;
        for(int i=cardsLitCountBefore;i<cardsLitCountBefore+_newRandomCardsAmnt;i++){
            Card c = ScriptableObject.CreateInstance<Card>();
            c.name = "card" + (i + 1);
            c.idName = "card" + (i + 1);
            c.displayName = "card " + (i + 1);
            c.description = "card " + (i + 1) + " description.";
            c.cardType = (cardType)Random.Range(0, 3 + 1);
            c.cost = Random.Range(0, 4 + 1);
            c.useRange = Random.Range(1, 5 + 1);
            c.useTime = Random.Range(1, 8 + 1);

            if(c.cardType==cardType.status){c.cost=1;c.useRange=0;c.useTime=Random.Range(0, 30+1);c.tickInMainHand=true;c.loopingTimes=-1;c.dismissable=true;}//c.unplayable=true;}
            if(c.cardType==cardType.instaUse){c.useRange=0;}//c.useTime=0;}
            if(c.cardType==cardType.structure){c.useTime=Random.Range(1, 30+1);}//c.useTime=0;}
            if(c.unplayable){c.description="Unplayable.\n"+c.description;}

            cardsList.Add(c);
        }
    }

    public void DrawCard(bool bypassHandSize=false){
        int _randCardId=Random.Range(0, cardsList.Count);
        DrawSpecificCard(cardsList[_randCardId].idName);
    }
    [Button("DrawCardOverride")]
    public void DrawCardOverride(){DrawCard(true);}
    public void DrawSpecificCard(string cardIdName,bool bypassHandSize=false){
        int _maxSize=handSize;
        if(handCapsAtMaxPlusLocked){_maxSize=handSize+_lockedCardsCount();}
        if(hand.Count<_maxSize||bypassHandSize){
            if((FindCard(cardIdName).cardType==cardType.status&&autoMoveStatusToLeftHand)&&(_leftHandNotFull())){
                AddCleanHandInfo(FindCard(cardIdName).idName);
                MoveCardToLeftHand(hand.Count-1);
            }else{
                AddCleanHandInfo(FindCard(cardIdName).idName);
            }
            UpdateHandUI();
        }else{Debug.LogWarning("Hand is full.");}
    }
    [Button("FillHand")]
    public void FillHand(){
        int _maxSize=handSize;
        if(handCapsAtMaxPlusLocked){_maxSize=handSize+_lockedCardsCount();}
        for(int i=hand.Count;i<_maxSize;i++){
            DrawCard();
        }
        UpdateUI();
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

        List<CardHandInfo> statusCards=new List<CardHandInfo>();
        if(autoMoveMainHandStatusToLeftHandOnReroll){
            if(hand!=null){if(hand.Count>0){
                for(int i=0;i<hand.Count;i++){
                    if(FindCard(hand[i].idName)!=null){
                        if(FindCard(hand[i].idName).cardType==cardType.status){
                            if(!lockedCards.Contains(hand[i])){
                                if(_leftHandNotFull()){MoveCardToLeftHand(i);}
                                else{
                                    if(keepStatusCardsWhenLeftHandFullOnReroll){
                                        statusCards.Add(CloneHandInfo(hand[i]));
                                    }
                                }
                            }
                        }
                    }
                }
            }}
        }
        hand=new List<CardHandInfo>(0);
        for(int i=0;i<lockedCards.Count;i++){//Readd locked cards
            hand.Add(CloneHandInfo(lockedCards[i]));
        }
        for(int i=0;i<statusCards.Count;i++){//Readd locked cards
            hand.Add(CloneHandInfo(statusCards[i]));
        }
        for(int i=hand.Count-1;i<handSize-1;i++){
            DrawCard();
        }
        UnselectCard();
        UpdateUI();
    }
    float minMaxCardRotationAngleNormal = 5f;
    float minMaxCardRotationAngle = 40f;
    float verticalOffsetFactor = 10f;
    float verticalOffsetFactorAngled = 40f;
    [Button("UpdateUI")]
    public void UpdateUI(){
        UpdateHandUI();
        UpdateLeftHandUI();
    }
    public void UpdateHandUI(){
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
            _cardEntity.SetProperties(i,false,card.idName,_angle);
            // _cardEntity.originalRot=_angle;
        }
    }
    public void UpdateLeftHandUI(){
        for(int i=leftHandLayoutGroup.childCount-1;i>=0;i--){
            Destroy(leftHandLayoutGroup.GetChild(i).gameObject);
        }
        for(int i=0;i<leftHand.Count;i++){
            CardHandInfo card = leftHand[i];
            GameObject _cardUi=Instantiate(cardUiPrefab,leftHandLayoutGroup);
            _cardUi.name="Card "+(i+1);
            CardEntity _cardEntity=_cardUi.GetComponent<CardEntity>();

            float verticalOffset=0;
            // if(angledHandLayout){
            //     float normalizedPos = (float)i / (Mathf.Clamp(hand.Count,2,99) - 1);
            //     verticalOffset = verticalOffsetFactorAngled * Mathf.Abs(normalizedPos - 0.5f);
            //     if(hand.Count<2){verticalOffset=0;}
            // }else{
            //     verticalOffset=verticalOffsetFactor*i;
            // }

            _cardEntity.transform.GetChild(0).GetComponent<RectTransform>().localPosition=new Vector3(0,-verticalOffset,0);

            float _angle=0;
            float _minMaxCardRotationAngleUsing=1;
            _angle=Mathf.Lerp(_minMaxCardRotationAngleUsing, -_minMaxCardRotationAngleUsing, (float)i / (Mathf.Clamp(hand.Count,2,99) - 1));
            if(hand.Count<2){_angle=0;}
            _cardEntity.SetProperties(i,true,card.idName,_angle);
            // _cardEntity.originalRot=_angle;
        }
    }

    public void SelectCard(int cardId,bool forceUnselect=false,bool allowInstaUse=false){
        if(cardId>=0&&cardId<hand.Count){
            Card _card=FindCard(hand[cardId].idName);
            if((_card.cardType==cardType.instaUse&&allowInstaUse)
            ||(_card.cardType==cardType.status&&!hand[cardId].isUnplayable&&allowInstaUse)
                ){lastSelectedCard=selectedCard;UseCard(cardId);UnselectCard();return;}
            if(!hand[cardId].isUnplayable){
                lastSelectedCard=selectedCard;
                if(forceUnselect){leftHandIsHovered=false;}//So that clicking on main hand unselects left hand from scrolling
                if(selectedCard==cardId&&clickingOnSameCardUnselects){UnselectCard();return;}
                if(selectedCard!=cardId&&selectedCard!=-1&&clickingOnDifferentCardUnselects&&forceUnselect){UnselectCard();return;}//For unselecting by pressing a different card
                selectedCard=cardId;
                selectedCardRef=_card;
            }else{UnselectCard();Debug.Log("Card is unplayable.");}
        }else{Debug.LogWarning(cardId+" is an incorrect hand card id!");}
    }
    public void SelectUp(){
        for(int i=selectedCard+1; i<hand.Count; i++){
            // if(FindCard(hand[i].idName).cardType != cardType.status){
            if(!hand[i].isUnplayable){
                SelectCard(i);
                return;
            }
        }

        if(infiniteScroll){
            for(int i=0; i<hand.Count; i++){
                // if(FindCard(hand[i].idName).cardType != cardType.status){
                if(!hand[i].isUnplayable){
                    SelectCard(i);
                    return;
                }
            }
        }
    }
    public void SelectDown(){
        if(selectedCard==-1&&hand.Count>1){
            // if(FindCard(hand[hand.Count-1].idName).cardType != cardType.status){
            if(!hand[hand.Count-1].isUnplayable){
                SelectCard(hand.Count-1);
            }//else{selectedCard=hand.Count-1;}
        }

        for(int i=selectedCard-1; i>=0; i--){
            // if(FindCard(hand[i].idName).cardType != cardType.status){
            if(!hand[i].isUnplayable){
                SelectCard(i);
                return;
            }
        }

        if(infiniteScroll){
            for(int i=hand.Count-1; i>=0; i--){
                // if(FindCard(hand[i].idName).cardType != cardType.status){
                if(!hand[i].isUnplayable){
                    SelectCard(i);
                    return;
                }
            }
        }
    }
    public void UnselectCard(){
        selectedCard=-1;
        selectedCardRef=null;
    }


    public void SelectLeftHandUp(){
        if(leftHandHoveredId+1<leftHand.Count){
            UnselectCard();
            leftHandHoveredId=leftHandHoveredId+1;
            return;
        }
        // for(int i=leftHandHoveredId+1; i<leftHand.Count; i++){
        //     leftHandHoveredId=i;
        //     return;
        // }

        if(infiniteScroll){
            UnselectCard();
            leftHandHoveredId=0;
            // for(int i=0; i<leftHand.Count; i++){
            //     leftHandHoveredId=i;
            //     return;
            // }
        }
    }
    public void SelectLeftHandDown(){
        if(leftHandHoveredId==-1&&leftHand.Count>1){
            leftHandHoveredId=leftHand.Count-1;
        }
        
        
        if(leftHandHoveredId-1>=0){
            UnselectCard();
            leftHandHoveredId=leftHandHoveredId-1;
            return;
        }
        // for(int i=leftHandHoveredId-1; i>=0; i--){
        //     leftHandHoveredId=i;
        //     return;
        // }

        if(infiniteScroll){
            UnselectCard();
            leftHandHoveredId=leftHand.Count-1;
            // for(int i=leftHand.Count-1; i>=0; i--){
            //     leftHandHoveredId=i;
            //     return;
            // }
        }
    }

    [Button("UseSelectedCardBypass")]
    public void UseSelectedCardBypass(){if(selectedCard!=-1)UseCard(selectedCard,true);}
    public void UseSelectedCard(bool bypassUnplayable=false){if(selectedCard!=-1)UseCard(selectedCard,bypassUnplayable);}
    public void UseSelectedCardAtPos(Vector2 pos,bool bypassUnplayable=false){if(selectedCard!=-1)UseCardAtPos(selectedCard,pos,bypassUnplayable);}
    public void UseCard(int cardId,bool bypassUnplayable=false){UseCardAtPos(cardId,Player.instance.transform.position,bypassUnplayable);}
    public void UseCardAtPos(int cardId,Vector2 pos,bool bypassUnplayable=false){
        bool _cardWasUsed=false;
        if(cardId>=0&&cardId<hand.Count){
            Card card=FindCard(hand[cardId].idName);
            if(energy>=hand[cardId].costCurrent&&(!lockingBlocksUse||(lockingBlocksUse&&!hand[cardId].locked))){
                if(_leftHandNotFull()){
                    // if(card.cardType!=cardType.status){
                    if(!hand[cardId].isUnplayable||bypassUnplayable){
                        energy-=hand[cardId].costCurrent;
                        CardFunctionsManager.instance.CardFunction(true,hand[cardId].idName,pos);
                        if(autoRefillOnUse){FillHand();}
                        if(instaRerollOnUse){RerollHand();}
                        MoveCardToLeftHand(cardId);
                        UpdateUI();
                        _cardWasUsed=true;
                    }else{
                        Debug.Log("Card is unplayable.");
                    }
                }else{
                    Debug.LogWarning("Left hand full.");
                }
            }else{
                if(energy<hand[cardId].costCurrent){
                    Debug.Log("You broke");
                }
                if(lockingBlocksUse&&hand[cardId].locked){
                    Debug.Log("Its locked");
                }
            }


            if(selectLastCardAfterUsing){
                if(lastSelectedCard>=0&&lastSelectedCard<hand.Count){SelectCard(lastSelectedCard);}
            }
            if(unselectCardsAfterUsing){
                if(_cardWasUsed){UnselectCard();}
            }else{
                if(_leftHandNotFull()){
                    selectedCardRef=null;//to leave the id as it was but refresh the reference
                    if(selectedCard>=hand.Count){selectedCard=hand.Count-1;}
                }
            }
            if(hand.Count<=0&&autoRefillOnEmptyHand){
                FillHand();
            }
            return;
        }else{Debug.LogWarning(cardId+" is an incorrect hand card id!");}
    }
    public void LockSelectedCard(){LockCard(selectedCard);}
    public void LockCard(int cardId){
        if(cardId>=0&&cardId<hand.Count){
            if(_lockedCardsNotCapped()){
                if(locks>0||hand[cardId].beenLocked){
                    if(!hand[cardId].beenLocked)locks-=1;

                    hand[cardId].locked=true;
                    hand[cardId].beenLocked=true;
                }else if(locks<=0){
                    Debug.Log("No locks left.");
                }
            }else{
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
            if(!hand[cardId].locked&&!_lockedCardsNotCapped()){
                Debug.Log("Locked cards capped.");
            }
            if(hand[cardId].locked||(!hand[cardId].locked&&_lockedCardsNotCapped())){
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

    public void MoveCardToLeftHand(int cardId){
        CloneCardToLeftHand(cardId);
        RemoveCardFromHand(cardId);
    }
    public void CloneCardToLeftHand(int cardId){
        leftHand.Add(CloneHandInfo(hand[cardId]));
        Card cardInfo=FindCard(hand[cardId].idName);
        if(leftHand[leftHand.Count-1].useTimer==-1)leftHand[leftHand.Count-1].useTimer=cardInfo.useTime;//Only set the timer if not already counting from main hand
    }

    public CardHandInfo AddCleanHandInfo(string cardIdName){
        Card _card=FindCard(cardIdName);
        CardHandInfo newCard=new CardHandInfo(){
            idName=cardIdName,
            useTimer=-1,
            costCurrent=_card.cost,
            isUnplayable=_card.unplayable,
            inactive=false,
            locked=false,
        };
        if(_card.tickInMainHand){
            newCard.useTimer=_card.useTime;
        }
        hand.Add(newCard);
        return hand[hand.Count-1];
    }
    public void RemoveCardFromHand(int cardId){hand.RemoveAt(cardId);UpdateHandUI();}
    public void RemoveCardFromLeftHand(int cardId){leftHand.RemoveAt(cardId);UpdateLeftHandUI();}

    public void CleanUsingCardList(){
        leftHand=new List<CardHandInfo>(0);
        UpdateUI();
    }
    public void AddEnergy(int amnt){energy+=amnt;}
    public void AddLocks(int amnt){locks+=amnt;}
    
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
    public int _lockedCardsCount(){return hand.FindAll(x=>x.locked).Count;}
    public bool _lockedCardsNotCapped(){return _lockedCardsCount()<lockedCardsCap||lockedCardsCap==-1;}
    public bool _leftHandNotFull(){return leftHand.Count<leftHandCap||leftHandCap==-1;}
}

[System.Serializable]
public class CardHandInfo{
    public string idName;
    public int costCurrent;
    public bool isUnplayable;
    public float useTimer=-1;
    public int loopedTimes;
    public bool inactive;
    public bool locked;
    public bool beenLocked;
}
public enum cardType{instaUse,ability,structure,status}
[System.Serializable]
public class CardTypeColor{
    [SerializeField] public cardType cardType;
    [SerializeField] public Color color=Color.white;
}