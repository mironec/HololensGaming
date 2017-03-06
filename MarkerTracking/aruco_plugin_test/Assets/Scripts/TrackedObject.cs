using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedObject : MonoBehaviour {
    public int markerId;

    public ArucoRunner trackingRunner;
    
	void Start () {
        trackingRunner.onDetectionRun += onDetectionRun;
	}

    private void onDetectionRun() {
        if (trackingRunner.poseDict.ContainsKey(markerId)) {
            gameObject.SetActive(true);
            PoseData pose = trackingRunner.poseDict[markerId];
            gameObject.transform.localPosition = pose.pos;
            gameObject.transform.localRotation = pose.rot;
        }
        else {
            gameObject.SetActive(false);
        }
    }
}
