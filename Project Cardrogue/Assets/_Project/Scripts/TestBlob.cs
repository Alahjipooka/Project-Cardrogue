using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Sirenix.OdinInspector;

public class TestBlob : MonoBehaviour{
    [SerializeField] bool randomizeColor=true;
    [SerializeField] float fadeAfter=5f;
    [Sirenix.OdinInspector.ReadOnly][SerializeField] float fadeAfterTimer;
    SpriteRenderer spr;
    void Start(){
        fadeAfterTimer=fadeAfter;
        spr=GetComponent<SpriteRenderer>();
        if(randomizeColor)SetColor(new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f)));
    }
    void Update(){
        if(fadeAfterTimer>0){fadeAfterTimer-=Time.deltaTime;}
        else{Destroy(gameObject);}
        spr.color=new Color(spr.color.r,spr.color.g,spr.color.b,(fadeAfterTimer/fadeAfter));
    }
    public void SetColor(Color _color){spr.color=_color;}
}
