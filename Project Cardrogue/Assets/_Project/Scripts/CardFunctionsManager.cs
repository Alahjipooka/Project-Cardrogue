using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public class CardFunctionsManager : MonoBehaviour{
    public static CardFunctionsManager instance;
    [SerializeField] GameObject testBlob;
    void Start(){instance=this;}
    public void CardFunction(bool initialUse,string cardIdName,Vector2 pos){
        Card card=CardManager.instance.FindCard(cardIdName);
        // GameObject _testBlob=Instantiate(testBlob,pos,Quaternion.identity);
        GameObject _testBlob = null;

        switch(cardIdName){
            case "bomb":
                if(initialUse){
                    _testBlob = Instantiate(testBlob, pos, Quaternion.identity);
                    _testBlob.GetComponent<TestBlob>().SetColor(Color.black);
                    _testBlob.GetComponent<TestBlob>().SetTime(card.useTime);
                }
            break;
            case "rifle":
                if(initialUse){
                    StartCoroutine(ShootRifleI(card.useTime,0));
                }
            break;
            case "turret":
                if(initialUse){
                    _testBlob = Instantiate(testBlob, pos, Quaternion.identity);
                    _testBlob.GetComponent<TestBlob>().SetColor(Color.red);
                    _testBlob.GetComponent<TestBlob>().SetTime(card.useTime);
                }
            break;
            case "wall":
                if(initialUse){
                    _testBlob = Instantiate(testBlob, pos, Quaternion.identity);
                    _testBlob.GetComponent<TestBlob>().SetColor(Color.yellow);
                    _testBlob.GetComponent<TestBlob>().SetTime(card.useTime);
                }
            break;
            case "toxicCloud":
                if(initialUse){
                    _testBlob = Instantiate(testBlob, pos, Quaternion.identity);
                    _testBlob.GetComponent<TestBlob>().SetColor(Color.green);
                    _testBlob.GetComponent<TestBlob>().SetTime(card.useTime);
                }
            break;
            case "energyRegen":
                if(!initialUse){
                    CardManager.instance.AddEnergy(1);
                }
            break;
            case "medkit":
                if(!initialUse){
                    Debug.Log("Player healed by: "+GameRules.instance.medkitCardHealAmnt);
                }
            break;
            case "toolkit":
                if(initialUse){
                    CardManager.instance.AddLocks(GameRules.instance.toolkitCardLocksAdd);
                }
            break;
            default:
                Debug.LogWarning(cardIdName+" does not have a function.");
            break;
        }
    }
    IEnumerator ShootRifleI(float useTime,float timePassed){
        if(timePassed<useTime){
            float fireRate=GameRules.instance.rifleCardFireRate;
            yield return new WaitForSecondsRealtime(fireRate);
            Debug.Log("ShootRifle "+System.Math.Round(timePassed,2)+" / "+System.Math.Round(useTime,2));
            GameObject _testBlob = Instantiate(testBlob, new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,Camera.main.ScreenToWorldPoint(Input.mousePosition).y,0), Quaternion.identity);
            Debug.Log("Made testblob at: "+Camera.main.ScreenToWorldPoint(Input.mousePosition));
            _testBlob.GetComponent<TestBlob>().SetColor(Color.red);
            _testBlob.GetComponent<TestBlob>().SetTime(1f);
            yield return StartCoroutine(ShootRifleI(useTime,timePassed+fireRate));
        }
    }
}
