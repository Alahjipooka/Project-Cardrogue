using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName="Card")]
public class Card : ScriptableObject{
    public string idName;
    public string displayName;
    [TextArea]public string description;
    public cardType cardType;
    public int cost;
    private bool IsNotStatus() => cardType != cardType.status;
    private bool IsNotInstaUse() => cardType != cardType.status && cardType != cardType.instaUse;
    private bool IsNotStatusOrInstaUse() => cardType != cardType.status && cardType != cardType.instaUse;
    public float useRange=3f;
    public float useTime=2f;
    public bool tickInMainHand=false;
    public int loopingTimes;
    public bool unplayable;
    public bool toggleable=true;
    public bool dismissable;
}
