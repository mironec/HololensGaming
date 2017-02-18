using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ArucoRunner : MonoBehaviour {
    public CameraProvider camProvider;
    public float markerSize;
    public int sizeReduce;

    public Dictionary<int, PoseData> poseDict;

    public event Action onDetectionRun;

    // Use this for initialization
    public void init () {
        int imgWidth;
        int imgHeight;
        float[] camParams;

        camProvider.init(out imgWidth, out imgHeight, out camParams);
            //Test if we got a valid image
        if(imgWidth > 0 && imgHeight > 0) {
            ArucoTracking.init(imgWidth, imgHeight, markerSize, camParams, sizeReduce);
        }
	}

    public void runDetect() {
        if (!ArucoTracking.lib_inited) return;

        Color32[] img_data = camProvider.getImage();
        ArucoTracking.detect_markers(img_data);

        poseDict = ArucoTrackingUtil.createUnityPoseData(ArucoTracking.marker_count, ArucoTracking.ids, ArucoTracking.rvecs, ArucoTracking.tvecs);

        onDetectionRun.Invoke();
    }
}
