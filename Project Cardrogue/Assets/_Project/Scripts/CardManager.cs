using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class CardManager : MonoBehaviour{
    public static CardManager instance;
    [AssetsOnly][SerializeField] GameObject cardUiPrefab;
    [SerializeField] public bool angledCardsLayout=false;
    [SerializeField] RectTransform handLayoutGroup;
    [SerializeField] int _newRandomCardsAmnt=10;
    public int handSize=5;
    public int energy=10;
    public int energyMax=20;
    public float energyRegenTime=5f;
    public int energyRegenRate=1;
    public int selectedCard=-1;
    public Card selectedCardRef=null;
    public bool autoRefillOnEmptyHand=true;
    [DisableIf("@this.instaRerollOnUse")]public bool autoRefillOnUse=false;
    [DisableIf("@this.autoRefillOnUse")]public bool instaRerollOnUse=false;

    public CardTypeColor[] cardTypesColors;
    public List<Card> cardsList;
    public List<Card> hand;
    void Awake(){
        if(instance!=null){Destroy(gameObject);}else{instance=this;/*DontDestroyOnLoad(gameObject);*/gameObject.name=gameObject.name.Split('(')[0];}
    }
    void Start(){
        RandomizeCardList();
        RerollHand();
        energy=100;energyMax=100;
        StartCoroutine(RegenerateEnergyI());
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.R)){RerollHand();}
        if(Input.GetKeyDown(KeyCode.F)){FillHand();}
        if(Input.GetKeyDown(KeyCode.Space)){UseSelectedCard();}
        if(Input.GetKeyDown(KeyCode.U)){UpdateUiWithHand();}
        if(Input.GetKeyDown(KeyCode.C)){RandomizeCardList();}
        if(Input.GetKeyDown(KeyCode.E)){energy+=10;}

        if(selectedCardRef==null&&selectedCard!=-1){
            selectedCardRef=FindCardInHand(hand[selectedCard].idName);
        }
    }

    [Button("RandomizeCardList")]
    public void RandomizeCardList(){
        cardsList=new List<Card>(0);
        for(int i=0;i<_newRandomCardsAmnt;i++){
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
        }
    }

    [Button("FillHand")]
    public void FillHand(){
        for(int i=hand.Count;i<handSize;i++){
            int _randCardId=Random.Range(0, cardsList.Count);
            hand.Add(CloneCard(cardsList[_randCardId]));
        }
        UpdateUiWithHand();
    }
    [Button("RerollHand")]
    public void RerollHand(){
        // hand=null;
        hand=new List<Card>(0);
        // hand=new List<Card>(handSize);
        for(int i=0;i<handSize;i++){
            int _randCardId=Random.Range(0, cardsList.Count);
            hand.Add(CloneCard(cardsList[_randCardId]));
        }
        UpdateUiWithHand();
    }
    // GameObject cardUiTemplate=null;
    float minMaxCardRotationAngleNormal = 5f;
    float minMaxCardRotationAngle = 40f;
    float verticalOffsetFactor = 10f;
    float verticalOffsetFactorAngled = 40f;
    public void UpdateUiWithHand(){
        // if(cardUiTemplate==null){cardUiTemplate=Instantiate(handLayoutGroup.GetChild(0).gameObject);}
        // for(int i=handLayoutGroup.childCount-1;i>0;i--){
        for(int i=handLayoutGroup.childCount-1;i>=0;i--){
            Destroy(handLayoutGroup.GetChild(i).gameObject);
        }

        for(int i=0;i<hand.Count;i++){
            Card card = hand[i];
            GameObject _cardUi=Instantiate(cardUiPrefab,handLayoutGroup);
            _cardUi.name="Card "+(i+1);
            CardEntity _cardEntity=_cardUi.GetComponent<CardEntity>();

            float verticalOffset=0;
            if(angledCardsLayout){
                float normalizedPos = (float)i / (Mathf.Clamp(hand.Count,2,99) - 1);
                verticalOffset = verticalOffsetFactorAngled * Mathf.Abs(normalizedPos - 0.5f);
                if(hand.Count<2){verticalOffset=0;}
            }else{
                verticalOffset=verticalOffsetFactor*i;
            }

            _cardEntity.transform.GetChild(0).GetComponent<RectTransform>().localPosition=new Vector3(0,-verticalOffset,0);

            float _angle=0;
            if(angledCardsLayout){
                _angle=Mathf.Lerp(minMaxCardRotationAngle, -minMaxCardRotationAngle, (float)i / (Mathf.Clamp(hand.Count,2,99) - 1));
                if(hand.Count<2){_angle=0;}
            }else{
                _angle=Mathf.Lerp(minMaxCardRotationAngleNormal, -minMaxCardRotationAngleNormal, (float)i / (Mathf.Clamp(hand.Count,2,99) - 1));
                if(hand.Count<2){_angle=0;}
            }
            _cardEntity.SetProperties(i,card.idName,_angle);
            // _cardEntity.originalRot=_angle;
        }
        // Destroy(cardUiTemplate);
    }
    public void SelectCard(int cardId){
        if(selectedCard==cardId){selectedCard=-1;return;}
        if(selectedCard!=cardId&&selectedCard!=-1){selectedCard=-1;return;}//For unselecting by pressing a different card
        selectedCard=cardId;
        selectedCardRef=FindCardInHand(hand[selectedCard].idName);
    }
    // public void UseCardId(int cardId){UseCard(hand[cardId]);}
    // public void UseCardIdName(string cardNameId){UseCard(cardsList.Find(x=>x.idName==cardNameId));}
    // public void UseCard(Card card){
    //     if(energy>=card.cost){
    //         if(card.cardType!=cardType.passive){
    //             // hand.RemoveAt(cardId);
    //             hand.RemoveAt(hand.FindIndex(x=>x.idName==card.idName));
    //             if(instaRerollOnUse){RerollHand();}
    //             else{UpdateUiWithHand();}
    //         }
    //     }else{}
    // }
    [Button("UseSelectedCard")]
    public void UseSelectedCard(){if(selectedCard!=-1)UseCard(selectedCard);}
    public void UseCard(int cardId){
        if(cardId>=0&&cardId<hand.Count){
            Card card=hand[cardId];
            if(energy>=card.cost){
                if(card.cardType!=cardType.passive){
                    hand.RemoveAt(cardId);
                    energy-=card.cost;
                    if(autoRefillOnUse){FillHand();}
                    if(instaRerollOnUse){RerollHand();}
                    else{UpdateUiWithHand();}
                }
            }else{Debug.Log("You broke");}
            selectedCard=-1;
            if(hand.Count<=0&&autoRefillOnEmptyHand){
                RerollHand();
            }
        }else{Debug.LogWarning(cardId+" is an incorrect card id!");}
    }

    IEnumerator RegenerateEnergyI(){
        yield return new WaitForSeconds(energyRegenTime);
        energy+=energyRegenRate;
        StartCoroutine(RegenerateEnergyI());
    }


    
    Card CloneCard(Card originalCard){
        string json = JsonUtility.ToJson(originalCard);
        Card clonedCard = JsonUtility.FromJson<Card>(json);
        return clonedCard;
    }
    Card CloneCardById(int originalCardId){
        string json = JsonUtility.ToJson(cardsList[originalCardId]);
        Card clonedCard = JsonUtility.FromJson<Card>(json);
        return clonedCard;
    }
    public Card FindCard(string cardIdName){
        return cardsList.Find(x=>x.idName==cardIdName);
    }
    public Card FindCardInHand(string cardIdName){
        return hand.Find(x=>x.idName==cardIdName);
    }

    void OnDestroy(){
        // if(cardUiTemplate!=null)Destroy(cardUiTemplate);
    }
}

[System.Serializable]
public class Card{
    public string idName;
    public string displayName;
    [TextArea]public string description;
    public cardType cardType;
    public int cost;
    private bool IsNotPassiveOrInstaUse() => cardType != cardType.passive && cardType != cardType.instaUse;
    private bool IsNotInstaUse() => cardType != cardType.instaUse;
    // [ShowIf("@!this.cardType.Equals(cardType.passive)&&!this.cardType.Equals(cardType.instaUse)")]public float useRange=3f;
    [ShowIf("IsNotPassiveOrInstaUse")]public float useRange=3f;
    // [ShowIf("@this.cardType.Equals(cardType.ability)")]public float useTime=2f;
    [ShowIf("IsNotPassiveOrInstaUse")]public float useTime=2f;
}
public enum cardType{instaUse,ability,structure,passive}
[System.Serializable]
public class CardTypeColor{
    [SerializeField] public cardType cardType;
    [SerializeField] public Color color=Color.white;
}