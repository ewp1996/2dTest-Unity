using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void startFunction()
    {
        SceneManager.LoadScene("GameScene");
    }

    //haven't bothered with making a quit button but here is the code for if i decide to implement one
    //create a quit button, add onClick method attach this script and choose the quitGame function 
    //public void quitGame()
    //{
     //   Application.Quit();
   // }
}


