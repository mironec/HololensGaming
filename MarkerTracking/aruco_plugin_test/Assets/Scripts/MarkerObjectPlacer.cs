using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class MarkerObjectPlacer : MonoBehaviour {
    //public MeshRenderer overlay_quad;
    public ArucoRunner trackingRunner;
    public Camera cam;
    
    public GameObject marker_quad_prefab;

    private List<GameObject> quad_instances;
    
    void Start () {
        trackingRunner.onDetectionRun += onDetectionRun;

        quad_instances = new List<GameObject>();

        //Taken from this stackoverflow answer:
        // http://stackoverflow.com/questions/36561593/opencv-rotation-rodrigues-and-translation-vectors-for-positioning-3d-object-in
        //float focal_y = camera_params[1];
        //webcam_fov = 2.0f * Mathf.Atan(0.5f * cam_height / focal_y) * Mathf.Rad2Deg;
        //cam.aspect = (float)cam_width / cam_height;
    }

    private void onDetectionRun() {

        //Add/remove quads to match how many we saw
        if (quad_instances.Count > ArucoTracking.marker_count) {
            //Clear out any instances we don't need anymore
            for (int i = quad_instances.Count - 1; i >= ArucoTracking.marker_count; i--) {
                GameObject.Destroy(quad_instances[i]);
                quad_instances.RemoveAt(i);
            }
        }
        else if (ArucoTracking.marker_count > quad_instances.Count) {
            int to_add = ArucoTracking.marker_count - quad_instances.Count;
            for (int i = 0; i < to_add; i++) {
                quad_instances.Add(make_marker_obj());
            }
        }

        for (int i = 0; i < ArucoTracking.marker_count; i++) {
            PoseData pose = trackingRunner.poseDict[ArucoTracking.ids[i]];
            quad_instances[i].transform.localPosition = pose.pos;
            quad_instances[i].transform.localRotation = pose.rot;
        }
    }

    GameObject make_marker_obj()
    {
        GameObject quad = GameObject.Instantiate(marker_quad_prefab);
        quad.transform.localScale = new Vector3(trackingRunner.markerSize, trackingRunner.markerSize, trackingRunner.markerSize);
        quad.transform.parent = cam.transform;
        return quad;
    }
}
