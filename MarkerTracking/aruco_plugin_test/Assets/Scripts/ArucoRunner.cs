using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ArucoRunner : MonoBehaviour {
    public CameraProvider camProvider;
    public float markerSize;
    public int sizeReduce;

    public Vector3 offset;

        //Low pass filter values. Defaults right now are such that no filtering happens
    public float positionLowPass = 0;
    public float rotationLowPass = 0;

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

    public virtual void runDetect() {
        if (!ArucoTracking.lib_inited) return;

        trackNewFrame();

        Dictionary<int, PoseData> newDict = ArucoTrackingUtil.createUnityPoseData(ArucoTracking.marker_count, ArucoTracking.ids, ArucoTracking.rvecs, ArucoTracking.tvecs);

        ArucoTrackingUtil.addCamSpaceOffset(newDict, offset); //Doing this first is important, since PoseDict also has positions with added offset
        ArucoTrackingUtil.posRotLowpass(poseDict, newDict, positionLowPass, rotationLowPass);
        
        poseDict = newDict;

        invokeOnDetectionRun();
    }

    protected void invokeOnDetectionRun() {
        onDetectionRun.Invoke();
    }

    protected void trackNewFrame() {
        Color32[] img_data = camProvider.getImage();
        ArucoTracking.detect_markers(img_data);
    }
}
