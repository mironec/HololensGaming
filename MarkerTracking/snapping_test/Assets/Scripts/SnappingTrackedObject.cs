using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnappingTrackedObject : MonoBehaviour {
    public int markerId;
    public PlayfieldPlacer playfieldPlacer;
    public float snappingDistance = 0.04f; //Distance below which a marker snaps to a plane. In meters.
    public float gridSize = 0.088f; //Size of squares on the plane grid, in meters.

    public ArucoRunner trackingRunner;

    //If this is specified, the marker will be placed in world space based on this camera. Otherwise, the object's local position is simply set to the pose data
    public Camera parentCamera;

    Quaternion baseRotation;

    void Start() {
        trackingRunner.onDetectionRun += onDetectionRun;
        baseRotation = transform.rotation;
    }

    private void onDetectionRun() {
        if (!trackingRunner.poseDict.ContainsKey(markerId)) return;

        PoseData pose = trackingRunner.poseDict[markerId];

        //Rotate by 90 along x axis to make the normal of the marker Y+, not Z+
        //This is the marker rotation, which is different to the rotation that we want our game object to have!
        //This distinction is important because the rotation of the game object may alter our coordinate system, so then we can't rely on a certain orientation relative to the marker for angle calculations later.
        //The object rotation is only brought in at the very end by applying the baseRotation locally to the markerRot, after that was adjusted for snapping etc
        Quaternion markerRot = parentCamera.transform.rotation * pose.rot * Quaternion.AngleAxis(90, Vector2.right);

        Vector3 posePos = pose.pos;
        posePos.z = -posePos.z;

        posePos = parentCamera.cameraToWorldMatrix.MultiplyPoint(posePos);

        if (playfieldPlacer.abstractGamePlanes != null) {
            foreach (PlaneInfo plane in playfieldPlacer.abstractGamePlanes) {
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
                Quaternion alignRot = Quaternion.FromToRotation(markerRot * Vector3.up, plane.rotation * Vector3.up);

                Quaternion planeSpaceMarkerRot = plane.inverseRotation * alignRot * markerRot;
                Vector3 markerPlaneDirection = planeSpaceMarkerRot * Vector3.forward;
                float angle = Mathf.Atan2(markerPlaneDirection.z, markerPlaneDirection.x);
                Debug.Log(angle);
                angle -= Mathf.PI / 2; //Make up point along Z+ on the plane

                angle = Mathf.Round(angle / (Mathf.PI / 2)) * (Mathf.PI / 2);

                markerRot = plane.rotation * Quaternion.AngleAxis(-angle * Mathf.Rad2Deg, Vector3.up);
            }
        }

        transform.position = posePos;

        //Now apply the object rotation before applying the marker rotation so that we can align our object in editor
        gameObject.transform.localRotation = markerRot * baseRotation;
    }
}
