using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    public int life;
    public int currentLife = 2;
    public Image[] healthSprites;
    public Sprite fullHealth;
    public Sprite halfHealth;
    public Sprite lastHit;



   
    private void Update()
    {
        healthSprites[0].sprite = fullHealth;
        healthSprites[1].sprite = halfHealth;
        healthSprites[2].sprite = lastHit;

        if (currentLife == 3)
        {
           
            healthSprites[1].enabled = false;
            healthSprites[2].enabled = false;
            healthSprites[0].enabled = true;
            
        }
        
        if(currentLife == 2)
        {
            
            healthSprites[1].enabled = true;
            healthSprites[2].enabled = false;
            healthSprites[0].enabled = false;
            
        }
        
        
        if(currentLife == 1)
        {
            
            healthSprites[1].enabled = false;
            healthSprites[2].enabled = true;
            healthSprites[0].enabled = false;
            
        }

    }

}
