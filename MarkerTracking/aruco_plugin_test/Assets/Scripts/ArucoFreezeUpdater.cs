using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class ArucoFreezeUpdater : MonoBehaviour, IInputClickHandler {
    public ArucoRunner runner;

    bool runDetect = true;

	// Use this for initialization
	void Start () {
        runner.init();
        InputManager.Instance.AddGlobalListener(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
        if (runDetect) {
            runner.runDetect();
        }       
	}

    public void OnInputClicked(InputEventData eventData) {
        runDetect = !runDetect;
    }
}
