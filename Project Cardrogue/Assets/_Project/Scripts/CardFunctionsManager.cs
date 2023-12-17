using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CardFunctionsManager : MonoBehaviour{
    public static CardFunctionsManager instance;
    [SerializeField] GameObject testBlob;
    void Start(){
        instance=this;
    }
    public void CardFunction(string cardIdName,Vector2 pos){
        GameObject _testBlob=Instantiate(testBlob,pos,Quaternion.identity);
    }
}
