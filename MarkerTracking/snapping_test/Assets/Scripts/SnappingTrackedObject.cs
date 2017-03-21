using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnappingTrackedObject : MonoBehaviour {
    public int markerId;
    public PlaneProvider planeProvider;
    public float snappingDistance = 0.04f; //Distance below which a marker snaps to a plane. In meters.
    public float gridSize = 0.088f; //Size of squares on the plane grid, in meters.

    public ArucoRunner trackingRunner;

    //If this is specified, the marker will be placed in world space based on this camera. Otherwise, the object's local position is simply set to the pose data
    public Camera parentCamera;

    void Start() {
        trackingRunner.onDetectionRun += onDetectionRun;
    }

    private void onDetectionRun() {
        if (!trackingRunner.poseDict.ContainsKey(markerId)) return;

        PoseData pose = trackingRunner.poseDict[markerId];

        Quaternion poseRot = pose.rot * Quaternion.AngleAxis(90, Vector2.right);

        Vector3 posePos = pose.pos;
        posePos.z = -posePos.z;

        posePos = parentCamera.cameraToWorldMatrix.MultiplyPoint(posePos);

        foreach (PlaneInfo plane in planeProvider.planes) {
            Vector3 planeSpacePos = posePos - plane.origin;

            planeSpacePos = plane.inverseRotation * planeSpacePos;

            if (planeSpacePos.x < -plane.size.x / 2 || plane.size.x / 2 < planeSpacePos.x) continue;
            if (planeSpacePos.z < -plane.size.y / 2 || plane.size.y / 2 < planeSpacePos.z) continue;
            if (Mathf.Abs(planeSpacePos.y) > snappingDistance) continue;

            planeSpacePos.y = 0;

            planeSpacePos.x = Mathf.Round(planeSpacePos.x / gridSize) * gridSize;
            planeSpacePos.z = Mathf.Round(planeSpacePos.z / gridSize) * gridSize;

            posePos = plane.rotation * planeSpacePos + plane.origin;
            poseRot = plane.rotation;
        }

        transform.position = posePos;

        gameObject.transform.localRotation = poseRot;
    }
}
