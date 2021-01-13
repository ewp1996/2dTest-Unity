using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImageSprite : MonoBehaviour
{
    private Transform player;

    //reference to the spriterenderer in the afterimage gameobject
    private SpriteRenderer SR;

    //reference to player game object spriterenderer so we know which sprite to get
    private SpriteRenderer playerSR;

    //need this so we can change the alpha of sprite over time
    private Color color;

    [SerializeField]
    //how long afterimage is active
    private float activeTime = 0.1f;

    //how long afterimage HAS been active
    private float timeActivated;

    //what alpha is at that time
    private float alpha;
    [SerializeField]
    //what alpha is set to when we enable the aferimage
    private float alphaSet = 0.8f;
    //how alpha changes on next iteration
    private float alphaMultiplier = 0.85f;


    //called every time gameobject is enabled, kind of like a start function
    private void OnEnable()
    {
        SR = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerSR = player.GetComponent<SpriteRenderer>();

        alpha = alphaSet;
        //get the correct sprite
        SR.sprite = playerSR.sprite;
        //set the afterimage position to player position
        transform.position = player.position;
        transform.rotation = player.rotation;
        timeActivated = Time.time;
    }


    private void Update()
    {
        alpha *= alphaMultiplier;
        color = new Color(1f, 1f, 1f, alpha);
        SR.color = color;

        if(Time.time >= (timeActivated + activeTime))
        {
            PlayerAfterImagePool.Instance.AddToPool(gameObject);
        }
    }
}
