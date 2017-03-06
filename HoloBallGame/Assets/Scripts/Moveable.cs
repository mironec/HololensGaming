using System;
using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class Moveable : MonoBehaviour, IManipulationHandler
{
    private bool isBeingManipulated = false;
    private Vector3 originalPosition;

    public bool isMoveable = true;

    public void OnManipulationCanceled(ManipulationEventData eventData)
    {
        isBeingManipulated = false;
    }

    public void OnManipulationCompleted(ManipulationEventData eventData)
    {
        isBeingManipulated = false;
    }

    public void OnManipulationStarted(ManipulationEventData eventData)
    {
        Debug.Log("Manipulation");
        isBeingManipulated = true;
        originalPosition = transform.position;
    }

    public void OnManipulationUpdated(ManipulationEventData eventData)
    {
        if(isMoveable) transform.position = originalPosition + eventData.CumulativeDelta;
    }

    // Use this for initialization
    void Start () {
        InputManager.Instance.AddGlobalListener(gameObject);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
	}
}
