using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OffsetArucoRunner : ArucoRunner {
    public Vector3 offset; //Offset of all pose position data, in camera space

    public Camera cam; //We need the camera object to know it's local coordinate space

    override public void runDetect() {
        if (!ArucoTracking.lib_inited) return;

        trackNewFrame();

        poseDict = ArucoTrackingUtil.createUnityPoseData(ArucoTracking.marker_count, ArucoTracking.ids, ArucoTracking.rvecs, ArucoTracking.tvecs);

        List<int> keys = new List<int>(poseDict.Keys);
        foreach(int key in keys) {
            PoseData data = poseDict[key];

            //data.pos += cam.transform.TransformDirection(offset);
            data.pos += offset;
            poseDict[key] = data;
        }

        invokeOnDetectionRun();
    }
}

