using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoPlayerParallax : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float textureSpeed;


    private void Update()
    {
        //Make background a material on a quad, use this line to access the texture(_MainTex) of the material, and offset it(move it) with the vector2 moving at the speed set in textureSpeed.
        GetComponent<Renderer>().material.SetTextureOffset("_MainTex", GetComponent<Renderer>().material.GetTextureOffset("_MainTex") + new Vector2(textureSpeed, 0) * Time.deltaTime);
    }
}