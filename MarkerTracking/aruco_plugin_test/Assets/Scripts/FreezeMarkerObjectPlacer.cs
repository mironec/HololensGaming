using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class FreezeMarkerObjectPlacer : MarkerObjectPlacer, IInputClickHandler {
    private bool applyTracking = true;

	// Use this for initialization
	override protected void Start () {
        base.Start();
        InputManager.Instance.AddGlobalListener(gameObject);
    }
	
    override protected void onDetectionRun() {
        if(applyTracking) {
            base.onDetectionRun();
        }
    }

    public void OnInputClicked(InputEventData eventData) {
        if(applyTracking) {
            foreach (GameObject instance in quad_instances) {
                Vector3 global_pos = instance.transform.position;
                instance.transform.parent = null;
                instance.transform.position = global_pos;
            }
            applyTracking = false;
        }
        else {
            foreach(GameObject instance in quad_instances) {
                instance.transform.parent = cam.transform; //Correct positioning will be handled by subsequent tracking
            }
            applyTracking = true;
        }
        
    }
}
