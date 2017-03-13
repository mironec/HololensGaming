using System;
using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class SelectPlayfield : MonoBehaviour, IInputClickHandler {
    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("I have been selected.");
    }

    void Start () {
		
	}
	
	void Update () {
		
	}
}
