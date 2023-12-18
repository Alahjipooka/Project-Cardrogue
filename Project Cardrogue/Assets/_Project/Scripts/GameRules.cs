using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRules : MonoBehaviour{     public static GameRules instance;
    [Header("Card Values")]
    public float medkitCardHealAmnt=10;
    public int toolkitCardLocksAdd=2;
    public float rifleCardFireRate=0.1f;
    public float speedCardBuffMult=1.25f;
    void Awake(){if(instance!=null){Destroy(gameObject);}else{instance=this;gameObject.name=gameObject.name.Split('(')[0];}}
}
