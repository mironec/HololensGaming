﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class frustrum_plane : MonoBehaviour {

        //The camera to be tracked. This object should be a parent of this camera
    public Camera tracking_camera;

        //Necessary because the actual fov of the camera gets changed by the unity holographic emulation -.-
    public float holo_fov = 17.537272544789690035827652800127f;
        //Set if you are running hololens emulation
    public bool holo_emulation = false;

	// Use this for initialization
	void Start () {
        if(!this.transform.IsChildOf(tracking_camera.transform))
        {
            Debug.LogWarning(this.name + " is not a child of " + tracking_camera.name + ", the camera it is tracking.");
        }
        float distance = transform.localPosition.z;
        float fov = tracking_camera.fieldOfView;
        if (holo_emulation) fov = holo_fov;
        float frustrum_height = 2.0f * distance * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float frustrum_width = frustrum_height * tracking_camera.aspect;

        Vector2 local_scale = transform.localScale;

        local_scale.x = frustrum_width;
        local_scale.y = frustrum_height;

        transform.localScale = local_scale;
	}
}
