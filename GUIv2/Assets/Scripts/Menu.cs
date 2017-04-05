using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

    public Canvas MainCanvas;
    public Canvas LevelCanvas;

    private void Awake()
    {
        LevelCanvas.gameObject.SetActive(false);
    }

    public void LevelOn()
    {
        LevelCanvas.gameObject.SetActive(true);
        MainCanvas.gameObject.SetActive(false);

    }

    public void ReturnOn()
    {
        LevelCanvas.gameObject.SetActive(false);
        MainCanvas.gameObject.SetActive(true);
    }

    public void CloseOn()
    {
        MainCanvas.gameObject.SetActive(false);
    }

    public void TerminateOn()
    {
        Application.Quit();
    }
/*
    public void LoadOn()
    {
        Application.LoadLevel(); 
    }
    */
}
