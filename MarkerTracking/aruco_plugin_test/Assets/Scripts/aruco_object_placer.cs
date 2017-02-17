using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class aruco_object_placer : MonoBehaviour {
    public int deviceNumber;
    public Texture2D test_img;
    public bool use_test_img = false;
    public MeshRenderer overlay_quad;
    public Camera cam;
    public bool hololens;
    public int desired_width;
    public int desired_height;

        //The real world marker size, in meters
    public float marker_size;
    public int size_reduce;
    
    public GameObject marker_quad_prefab;

    private List<GameObject> quad_instances;

    private WebCamTexture _webcamTexture;
    private int cam_width = -1;
    private int cam_height = -1;

    private float webcam_fov;

    private Vector2 resolution;
    
    private Color32[] colors;

    private float[] camera_params;
    
    void Start () {
        
        if (use_test_img)
        {
            cam_width = test_img.width;
            cam_height = test_img.height;

            overlay_quad.material.mainTexture = test_img;
            colors = test_img.GetPixels32();
        }
        else
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length > 0)
            {
                Debug.Log("Got devices");
                _webcamTexture = new WebCamTexture(devices[deviceNumber].name, desired_width, desired_height, 30);
                // Play the video source
                _webcamTexture.Play();
                cam_width = _webcamTexture.width;
                cam_height = _webcamTexture.height;

                Debug.Log(cam_width);
                Debug.Log(cam_height);

                //overlay.texture = _webcamTexture;
                overlay_quad.material.mainTexture = _webcamTexture;
                colors = new Color32[cam_width * cam_height];
            }
            else
            {
                Debug.Log("No webcam found!");
            }
        }

        if (cam_width != -1) {
            init_camera_params();
            ArucoTracking.init(cam_width, cam_height, marker_size, camera_params, size_reduce);
        }        

        quad_instances = new List<GameObject>();

        //Taken from this stackoverflow answer:
        // http://stackoverflow.com/questions/36561593/opencv-rotation-rodrigues-and-translation-vectors-for-positioning-3d-object-in
        //float focal_y = camera_params[1];
        //webcam_fov = 2.0f * Mathf.Atan(0.5f * cam_height / focal_y) * Mathf.Rad2Deg;
        //cam.aspect = (float)cam_width / cam_height;
    }

    void init_camera_params()
    {
        camera_params = new float[4 + 5];
        if(hololens)
        {
                //Hololens camera parameters based on camera photos, which are 1408x792, at 48 horizontal FOV
            camera_params[0] = 1.6226756644523603e+03f;
            camera_params[1] = 1.6226756644523603e+03f;
            //camera_params[2] = 6.2516688711209542e+02f;
            //camera_params[3] = 3.8018373700505418e+02f;
                //The opencv calibration program didn't emit useful values for some reason, but normally these are simply half of the image width/height
            camera_params[2] = 1408/2;
            camera_params[3] = 792/2;
            camera_params[4] = -5.6781211352631726e-03f;
            camera_params[5] = -1.1566538973188603e+00f;
            camera_params[6] = -1.3849725342370161e-03f;
            camera_params[7] = -3.9288657236615111e-03f;
            camera_params[8] = 9.4499768251174778e+00f;
        }
        else
        {
                //Parameters for a Macbook 15" webcam from 1280x720 image
            camera_params[0] = 1.0240612805194348e+03f;
            camera_params[1] = 1.0240612805194348e+03f;
            camera_params[2] = 1280 / 2;
            camera_params[3] = 720 / 2;
            //camera_params[2] = 6.3218846628075391e+02f;
            //camera_params[3] = 3.6227541578720428e+02f;
            camera_params[4] = 7.9272342555005190e-02f;
            camera_params[5] = -1.7557543937376724e-01f;
            camera_params[6] = 6.0915748810957840e-04f;
            camera_params[7] = -2.9391344753009105e-03f;
            camera_params[8] = 1.0650125708199540e-01f;
        }
    }

    GameObject make_marker_obj()
    {
        GameObject quad = GameObject.Instantiate(marker_quad_prefab);
        quad.transform.localScale = new Vector3(marker_size, marker_size, marker_size);
        quad.transform.parent = cam.transform;
        return quad;
    }

    void OnDestroy()
    {
        ArucoTracking.destroy();
    }
    
    // Update is called once per frame
    void Update () {
        if (!ArucoTracking.lib_inited) return;

        if (!use_test_img)
        {
            _webcamTexture.GetPixels32(colors);
        }
        
        ArucoTracking.detect_markers(colors);

        //Add/remove quads to match how many we saw
        if (quad_instances.Count > ArucoTracking.marker_count)
        {
            //Clear out any instances we don't need anymore
            for (int i = quad_instances.Count - 1; i >= ArucoTracking.marker_count; i--)
            {
                GameObject.Destroy(quad_instances[i]);
                quad_instances.RemoveAt(i);
            }
        }
        else if (ArucoTracking.marker_count > quad_instances.Count)
        {
            int to_add = ArucoTracking.marker_count - quad_instances.Count;
            for (int i = 0; i < to_add; i++)
            {
                quad_instances.Add(make_marker_obj());
            }
        }

        if (ArucoTracking.marker_count > 0)
        {
            // Extract vectors from data and apply transformation to objects
            for (int i = 0; i < ArucoTracking.marker_count; i++)
            {
                Vector3 tvec = new Vector3((float)ArucoTracking.tvecs[i * 3], (float)ArucoTracking.tvecs[i * 3 + 1], (float)ArucoTracking.tvecs[i * 3 + 2]);
                quad_instances[i].transform.localPosition = tvec;

                Vector3 rvec = new Vector3((float)ArucoTracking.rvecs[i * 3], (float)ArucoTracking.rvecs[i * 3 + 1], (float)ArucoTracking.rvecs[i * 3 + 2]);
               
                float theta = rvec.magnitude;
                rvec.Normalize();

                //the rvec from OpenCV is a compact axis-angle format. The direction of the vector is the axis, and the length of it is the angle to rotate about (i.e. theta)
                //From this stackoverflow answer: http://stackoverflow.com/questions/12933284/rodrigues-into-eulerangles-and-vice-versa
                Quaternion new_rot = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, rvec);
                quad_instances[i].transform.localRotation = new_rot;
            }
        }
	}
}
