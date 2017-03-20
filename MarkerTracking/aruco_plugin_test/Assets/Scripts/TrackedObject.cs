using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedObject : MonoBehaviour {
    public int markerId;

    public ArucoRunner trackingRunner;
        //If true, the object will not be deactivated even when the marker is not being detected
    public bool persist = false;
    
	void Start () {
        trackingRunner.onDetectionRun += onDetectionRun;
	}

    private void onDetectionRun() {
        if (trackingRunner.poseDict.ContainsKey(markerId)) {
            if(!persist) gameObject.SetActive(true);
            PoseData pose = trackingRunner.poseDict[markerId];
            gameObject.transform.localPosition = pose.pos;
            gameObject.transform.localRotation = pose.rot;
        }
        else {
            if (!persist) gameObject.SetActive(false);
        }
    }
}
