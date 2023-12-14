using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class CardEntity : MonoBehaviour{
    [ReadOnly]public string idName;
    void Start(){}
    public void SetProperties(string _idName=""){
        if(_idName!=""){idName=_idName;}
        Card card=CardManager.instance.cardsList.Find(x=>x.idName==this.idName);

        GetComponent<Image>().color=CardManager.instance.cardTypesColors[(int)card.cardType].color;
        transform.GetChild(0).GetComponent<TextMeshProUGUI>().text=card.displayName;
        transform.GetChild(1).GetComponent<TextMeshProUGUI>().text=card.description;
        transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text=card.cost.ToString();
    }
}
