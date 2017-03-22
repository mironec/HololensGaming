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

                //Find the rotation such that the normal vectors of the marker and plane are aligned. This way the marker object sits parallel to the plane, but still retains rotation around that axis
            Quaternion alignRot = Quaternion.FromToRotation(poseRot * Vector3.up, plane.rotation * Vector3.up);
            //poseRot = alignRot * poseRot;

            Quaternion planeSpaceMarkerRot = plane.inverseRotation * alignRot * poseRot;
            Vector3 markerPlaneDirection = planeSpaceMarkerRot * Vector3.forward;
            float angle = Mathf.Atan2(markerPlaneDirection.z, markerPlaneDirection.x);
            angle -= Mathf.PI / 2; //Make up point along Z

            angle = Mathf.Round(angle / (Mathf.PI / 2)) * (Mathf.PI / 2);
            
            Quaternion angleSnapRot = plane.rotation * Quaternion.AngleAxis(-angle * Mathf.Rad2Deg, Vector3.up);
            poseRot = angleSnapRot;
        }

        transform.position = posePos;

        gameObject.transform.localRotation = poseRot;
    }
}
