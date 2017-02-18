using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ArucoUpdateRunner : MonoBehaviour {
    public ArucoRunner runner;
    
    private void Awake() {
        runner.init();
    }

    private void Update() {
        runner.runDetect();
    }

    private void OnDestroy() {
        ArucoTracking.destroy();
    }
}
