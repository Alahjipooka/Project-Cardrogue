using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Cinemachine.Utility;
using Cinemachine;

public class Player : MonoBehaviour{
    [SerializeField] public float runSpeed = 20.0f;
    [SerializeField] public float moveLimiter = 0.7f;
    [SerializeField] public float cameraOffsetChangeSpeed = 1f;
    [ChildGameObjectsOnly][SerializeField] public GameObject useParameter;
    [SerializeField] float useParameterBaseScale=10f;

    float horizontal;
    float vertical;
    [SerializeField]float last_vertical;
    [SerializeField]float last_horizontal;
    Rigidbody2D rb;
    CinemachineCameraOffset cm_cameraoffset;


    void Start(){
        rb=GetComponent<Rigidbody2D>();
        cm_cameraoffset=FindObjectOfType<CinemachineCameraOffset>();
        last_vertical=-1;//Face down
    }

    void Update(){
        horizontal = Input.GetAxisRaw("Horizontal"); // -1 is left
        vertical = Input.GetAxisRaw("Vertical"); // -1 is down

        if(horizontal!=0||vertical!=0){
            if(last_vertical==0){last_horizontal=horizontal;last_vertical=0;}
            if(last_horizontal==0){last_vertical=vertical;last_horizontal=0;}
        }
        if(last_horizontal!=0||last_vertical!=0){
            float _step=Time.deltaTime*cameraOffsetChangeSpeed;
            cm_cameraoffset.m_Offset=Vector3.MoveTowards(cm_cameraoffset.m_Offset,new Vector3(last_horizontal,last_vertical,0),_step);
        }

        if(CardManager.instance.selectedCard!=-1&&CardManager.instance.selectedCardRef!=null){
            float _step=Time.fixedDeltaTime*5f;
            useParameter.transform.localScale=Vector2.MoveTowards(useParameter.transform.localScale,new Vector2(useParameterBaseScale*CardManager.instance.selectedCardRef.useRange,useParameterBaseScale*CardManager.instance.selectedCardRef.useRange),_step);
            useParameter.transform.Rotate(new Vector3(0,0,2));
        }else{
            useParameter.transform.localScale=Vector2.zero;
        }
    }
    void FixedUpdate() {
        if (horizontal != 0 && vertical != 0) // Check for diagonal movement
        {
            // limit movement speed diagonally, so you move at 70% speed
            horizontal *= moveLimiter;
            vertical *= moveLimiter;
        } 

        rb.velocity = new Vector2(horizontal * runSpeed, vertical * runSpeed);
    }
}
