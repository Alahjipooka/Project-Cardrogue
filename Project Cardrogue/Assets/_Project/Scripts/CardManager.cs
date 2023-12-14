using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class CardManager : MonoBehaviour{
    public static CardManager instance;
    [SerializeField] RectTransform handLayoutGroup;
    [SerializeField] int _newRandomCardsAmnt=10;
    public int handSize=5;
    public int energy=10;
    public bool instaRerollOnUse=false;

    public CardTypeColor[] cardTypesColors;
    public List<Card> cardsList;
    public List<Card> hand;
    void Awake(){
        if(instance!=null){Destroy(gameObject);}else{instance=this;/*DontDestroyOnLoad(gameObject);*/gameObject.name=gameObject.name.Split('(')[0];}
    }
    void Start(){
        RandomizeCardList();
        RerollHand();
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.R)){RerollHand();}
        if(Input.GetKeyDown(KeyCode.U)){UpdateUiWithHand();}
        if(Input.GetKeyDown(KeyCode.C)){RandomizeCardList();}
        if(Input.GetKeyDown(KeyCode.E)){energy+=10;}
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
                cost = Random.Range(0, 4+1)
            });
        }
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
    public void UpdateUiWithHand(){
        GameObject cardUiTemplate=handLayoutGroup.GetChild(0).gameObject;
        for(int i=handLayoutGroup.childCount-1;i>0;i--){
            Destroy(handLayoutGroup.GetChild(i).gameObject);
        }
        for(int i=0;i<handSize;i++){
            Card card = hand[i];
            GameObject _cardUi=Instantiate(cardUiTemplate,handLayoutGroup);
            _cardUi.name="Card "+(i+1);
            CardEntity _cardEntity=_cardUi.GetComponent<CardEntity>();
            _cardEntity.SetProperties(card.idName);
            // _cardUi.GetComponent<Image>().color=cardTypesColors[(int)card.cardType].color;
            // _cardUi.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text=card.displayName;
            // _cardUi.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text=card.description;
            // _cardUi.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text=card.cost.ToString();
        }
        Destroy(cardUiTemplate);
    }
    public void SelectCard(int cardId){}
    public void UseCard(int cardId){
        Card card=hand[cardId];
        if(energy>=card.cost){
            if(card.cardType!=cardType.passive){
                hand.RemoveAt(cardId);
                if(instaRerollOnUse){RerollHand();}
                else{UpdateUiWithHand();}
            }
        }else{}
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
}

[System.Serializable]
public class Card{
    public string idName;
    public string displayName;
    [TextArea]public string description;
    public cardType cardType;
    public int cost;
}
public enum cardType{instaUse,ability,structure,passive}
[System.Serializable]
public class CardTypeColor{
    [SerializeField] public cardType cardType;
    [SerializeField] public Color color=Color.white;
}