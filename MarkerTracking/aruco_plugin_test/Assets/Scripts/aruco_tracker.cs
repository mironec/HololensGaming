using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class aruco_tracker : MonoBehaviour {
    // Original Video parameters
    public int deviceNumber;
    public Texture2D test_img;
    public bool use_test_img = false;
    public MeshRenderer overlay_quad;
    public Camera cam;
    
    public GameObject marker_quad_prefab;

    private List<GameObject> quad_instances;

    private WebCamTexture _webcamTexture;
    private int cam_width;
    private int cam_height;

    private float webcam_fov;

    [DllImport("aruco_plugin")]
    public static extern void init(int width, int height);

    [DllImport("aruco_plugin")]
    public static extern int detect_markers(IntPtr unity_img, ref int marker_count, ref IntPtr out_ids, ref IntPtr out_corners, ref IntPtr out_rvecs, ref IntPtr out_tvecs);

    [DllImport("aruco_plugin")]
    public static extern void set_debug_cb(IntPtr ptr);

    [DllImport("aruco_plugin")]
    public static extern void destroy();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PrintDelegate(string str);
    
    private bool dll_inited = false;

    private Vector2 resolution;

    // Use this for initialization
    void Start () {
        if(use_test_img)
        {
            cam_width = test_img.width;
            cam_height = test_img.height;
            init(cam_width, cam_height);
            dll_inited = true;

            overlay_quad.material.mainTexture = test_img;
        }
        else
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length > 0)
            {
                Debug.Log("Got devices");
                _webcamTexture = new WebCamTexture(devices[deviceNumber].name, 1280, 720, 30);
                // Play the video source
                _webcamTexture.Play();
                cam_width = _webcamTexture.width;
                cam_height = _webcamTexture.height;

                init(cam_width, cam_height);
                dll_inited = true;

                //overlay.texture = _webcamTexture;
                overlay_quad.material.mainTexture = _webcamTexture;
            }
            else
            {
                Debug.Log("No webcam found!");
            }
        }

        PrintDelegate plugin_delegate = new PrintDelegate(PluginDebugLog);
        IntPtr delegate_ptr = Marshal.GetFunctionPointerForDelegate(plugin_delegate);
        set_debug_cb(delegate_ptr);

        quad_instances = new List<GameObject>();

        //todo: fix the duplicate of cam_width/cam_height and resolution
        resolution = new Vector2(1280, 720);

        //Taken from this stackoverflow answer:
        // http://stackoverflow.com/questions/36561593/opencv-rotation-rodrigues-and-translation-vectors-for-positioning-3d-object-in
        float focal_y = 1.0240612805194348e+03f;
        webcam_fov = 2.0f * Mathf.Atan(0.5f * resolution.y / focal_y) * Mathf.Rad2Deg;
        cam.aspect = resolution.x / resolution.y;
    }

    GameObject make_marker_quad()
    {
        GameObject quad = GameObject.Instantiate(marker_quad_prefab);
        quad.transform.localScale = new Vector3(0.088f, 0.088f, 0.088f); //Matches the real world marker scale, 1 unit = 1m
        quad.transform.parent = cam.transform;
        return quad;
    }

    void OnDestroy()
    {
        if(dll_inited) destroy();
    }

    // Update is called once per frame
    void Update () {
        Color32[] colors;
        if(use_test_img)
        {
            colors = test_img.GetPixels32();
        }
        else
        {
            colors = _webcamTexture.GetPixels32();
        }
        IntPtr imageHandle = getImageHandle(colors);

        int marker_count = 0;
        IntPtr out_ids = IntPtr.Zero;
        IntPtr out_corners = IntPtr.Zero;
        IntPtr out_rvecs = IntPtr.Zero;
        IntPtr out_tvecs = IntPtr.Zero;

        detect_markers(imageHandle, ref marker_count, ref out_ids, ref out_corners, ref out_rvecs, ref out_tvecs);

            //Add/remove quads to match how many we saw
        if (quad_instances.Count > marker_count)
        {
            //Clear out any instances we don't need anymore
            for (int i = quad_instances.Count - 1; i >= marker_count; i--)
            {
                GameObject.Destroy(quad_instances[i]);
                quad_instances.RemoveAt(i);
            }
        }
        else if (marker_count > quad_instances.Count)
        {
            int to_add = marker_count - quad_instances.Count;
            for (int i = 0; i < to_add; i++)
            {
                quad_instances.Add(make_marker_quad());
            }
        }

        if (marker_count > 0)
        {
                //Copy over data from plugin side to c# managed arrays
            int[] ids = new int[marker_count];
            Marshal.Copy(out_ids, ids, 0, marker_count);

            float[] corners = new float[marker_count * 8];
            Marshal.Copy(out_corners, corners, 0, marker_count * 8);

            double[] rvecs = new double[marker_count * 3];
            Marshal.Copy(out_rvecs, rvecs, 0, marker_count * 3);
                
            double[] tvecs = new double[marker_count * 3];
            Marshal.Copy(out_tvecs, tvecs, 0, marker_count * 3);
            
            // Extract vectors from data and apply transformation to objects
            for (int i = 0; i < marker_count; i++)
            {
                Vector3 tvec = new Vector3((float)tvecs[i * 3], (float)tvecs[i * 3 + 1], (float)tvecs[i * 3 + 2]);
                tvec.z *= webcam_fov / cam.fieldOfView;
                quad_instances[i].transform.localPosition = tvec;

                Vector3 rvec = new Vector3((float)rvecs[i * 3], (float)rvecs[i * 3 + 1], (float)rvecs[i * 3 + 2]);
               
                float theta = rvec.magnitude;
                rvec.Normalize();

                //the rvec from OpenCV is a compact axis-angle format. The direction of the vector is the axis, and the length of it is the angle to rotate about (i.e. theta)
                //From this stackoverflow answer: http://stackoverflow.com/questions/12933284/rodrigues-into-eulerangles-and-vice-versa
                Quaternion new_rot = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, rvec);
                quad_instances[i].transform.localRotation = new_rot;
            }
        }
	}

    private static IntPtr getImageHandle(object colors)
    {
        IntPtr ptr;
        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            ptr = handle.AddrOfPinnedObject();
        }
        finally
        {
            if (handle != default(GCHandle))
                handle.Free();
        }
        return ptr;
    }

    static void PluginDebugLog(string str)
    {
        Debug.Log("Aruco Plugin: " + str);
    }
}
