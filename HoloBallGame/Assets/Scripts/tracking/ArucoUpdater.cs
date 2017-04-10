using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArucoUpdater : MonoBehaviour {
    public ArucoRunner runner;

    public bool runDetection = true;

    public bool detectionDisabled = false;

    public Camera cam;
    public float maxAngularChange = 1;
    private Quaternion previousCamRot = Quaternion.identity;
    
    private void Awake() {
        if (isActiveAndEnabled) {
            runner.init();
        }
    }

    private void Update() {
        float camRotationDifference = Quaternion.Angle(cam.transform.rotation, previousCamRot);
        previousCamRot = cam.transform.rotation;

        if(camRotationDifference < maxAngularChange) {
            if (runDetection && !detectionDisabled) runner.runDetect();
        }
    }

    private void OnDestroy() {
        ArucoTracking.destroy();
    }

    public void disableDetection() {
        detectionDisabled = true;
    }

    public void enableDetection() {
        detectionDisabled = false;
    }
}
