using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class aruco_tracker : MonoBehaviour {
    // Original Video parameters
    public int deviceNumber;
    public RawImage overlay;
    public Camera cam;

    public bool apply_rotation = true;

    public GameObject marker_quad_prefab;
    public GameObject id_text_prefab;

    private List<GameObject> quad_instances;
    private List<GameObject> id_instances;

    private WebCamTexture _webcamTexture;
    private int cam_width;
    private int cam_height;

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
    private Vector2 cparams;

    // Use this for initialization
    void Start () {
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

            overlay.texture = _webcamTexture;

        }
        else
        {
            Debug.Log("No webcam found!");
        }

        PrintDelegate plugin_delegate = new PrintDelegate(PluginDebugLog);
        IntPtr delegate_ptr = Marshal.GetFunctionPointerForDelegate(plugin_delegate);
        set_debug_cb(delegate_ptr);

        quad_instances = new List<GameObject>();
        id_instances = new List<GameObject>();

        //todo: fix the duplicate of cam_width/cam_height and resolution
        resolution = new Vector2(1280, 720);
        cparams = new Vector2(6.3218846628075391e+02f, 3.6227541578720428e+02f); //Extracted from the camera matrix used in the plugin

        //This and the code in assigning the marker object a new position taken from this stackoverflow answer:
        // http://stackoverflow.com/questions/36561593/opencv-rotation-rodrigues-and-translation-vectors-for-positioning-3d-object-in
        float focal_y = 1.0240612805194348e+03f;
        float vfov = 2.0f * Mathf.Atan(0.5f * resolution.y / focal_y) * Mathf.Rad2Deg;
        cam.fieldOfView = vfov;
        cam.aspect = resolution.x / resolution.y;

        

    }

    GameObject make_marker_quad()
    {
        GameObject quad = GameObject.Instantiate(marker_quad_prefab);
        quad.transform.localScale = new Vector3(0.088f, 0.088f, 0.088f); //Matches the real world marker scale, 1 unit = 1m
        if(!apply_rotation)
        {
            quad.transform.localScale = new Vector3(0.02f, 0.005f, 0.01f);
        }
        return quad;
    }

    void OnDestroy()
    {
        if(dll_inited) destroy();
    }

    // Update is called once per frame
    void Update () {
		//if(Input.GetKeyDown(KeyCode.Space))
  //      {
            // Send video capture to ARUCO controller
            Color32[] colors = _webcamTexture.GetPixels32();
            IntPtr imageHandle = getImageHandle(colors);

            int marker_count = 0;
            IntPtr out_ids = IntPtr.Zero;
            IntPtr out_corners = IntPtr.Zero;
            IntPtr out_rvecs = IntPtr.Zero;
            IntPtr out_tvecs = IntPtr.Zero;

            detect_markers(imageHandle, ref marker_count, ref out_ids, ref out_corners, ref out_rvecs, ref out_tvecs);
            
            //Debug.Log("Markers found: " + marker_count);

                //Add/remove quads to match how many we saw
            if (quad_instances.Count > marker_count)
            {
                //Clear out any instances we don't need anymore
                for (int i = quad_instances.Count - 1; i >= marker_count; i--)
                {
                    GameObject.Destroy(quad_instances[i]);
                    quad_instances.RemoveAt(i);
                    GameObject.Destroy(id_instances[i]);
                    id_instances.RemoveAt(i);
                }
            }
            else if (marker_count > quad_instances.Count)
            {
                int to_add = marker_count - quad_instances.Count;
                for (int i = 0; i < to_add; i++)
                {
                    quad_instances.Add(make_marker_quad());
                    GameObject id_text = GameObject.Instantiate(id_text_prefab);
                    id_instances.Add(id_text);
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

                
                Vector3 image_center = new Vector3(0.5f, 0.5f, 0);
                Vector3 optical_center = new Vector3(0.5f + cparams.x / resolution.x, 0.5f + cparams.y / resolution.y, 0);

                //Print out all information on each marker, for debugging
                for (int i = 0; i < marker_count; i++)
                {
                    Vector3 tvec = new Vector3((float)tvecs[i*3], (float)tvecs[i*3 + 1], (float)tvecs[i*3 + 2]);
                    image_center.z = tvec.z;
                    optical_center.z = tvec.z;
                    tvec += cam.ViewportToWorldPoint(image_center) - cam.ViewportToWorldPoint(optical_center);
                    quad_instances[i].transform.position = tvec;
                    
                    if(apply_rotation)
                    {
                        Vector3 rvec = new Vector3((float)rvecs[i * 3], (float)rvecs[i * 3 + 1], (float)rvecs[i * 3 + 2]);
                        float theta = rvec.magnitude;
                        rvec.Normalize();

                        //the rvec from OpenCV is a compact axis-angle format. The direction of the vector is the axis, and the length of it is the angle to rotate about (i.e. theta)
                        //From this stackoverflow answer: http://stackoverflow.com/questions/12933284/rodrigues-into-eulerangles-and-vice-versa
                        Quaternion new_rot = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, rvec);

                        quad_instances[i].transform.rotation = new_rot;
                    }
                    else
                    {
                        quad_instances[i].transform.rotation = Quaternion.Euler(0, 180, 0);
                    }

                    GameObject text_object = id_instances[i];
                    text_object.transform.position = tvec;
                    TextMesh text_mesh = text_object.GetComponent<TextMesh>();
                    text_mesh.text = " ID: " + ids[i];
                    
                    
                    /*
                    Debug.Log("id: " + ids[i]);
                    Debug.Log("Corners: ");
                    for (int j = 0; j < 4; j++)
                    {
                        Debug.Log("" + j + ": " + corners[i * 8 + j * 2] + ", " + corners[i * 8 + j * 2 + 1]);
                    }

                    Debug.Log("rvec: " + rvecs[i * 3] + ", " + rvecs[i * 3 + 1] + ", " + rvecs[i * 3 + 2]);
                    Debug.Log("tvec: " + tvecs[i * 3] + ", " + tvecs[i * 3 + 1] + ", " + tvecs[i * 3 + 2]);
                    */
                }
                
                    //The following is debug code to visualize the rvec information from opencv better
                //Re-building the rotation matrix based on the rodrigues vector - see this docs page for the formula: http://docs.opencv.org/3.1.0/d9/d0c/group__calib3d.html#ga61585db663d9da06b68e70cfbf6a1eac
                //Vector3 f = new Vector3();
                //Vector3 u = new Vector3();
                //Vector3 r = new Vector3();

                //float cos_theta = Mathf.Cos(theta);
                //float one_minus_cos = 1.0f - cos_theta;
                //float sin_theta = Mathf.Sin(theta);

                //u.x = one_minus_cos * rvec.x * rvec.y + (sin_theta * -rvec.z);
                //u.y = cos_theta + one_minus_cos * rvec.y * rvec.y;
                //u.z = one_minus_cos * rvec.y * rvec.z + sin_theta * rvec.x;

                //f.x = one_minus_cos * rvec.x * rvec.z + sin_theta * rvec.y;
                //f.y = one_minus_cos * rvec.y * rvec.z + sin_theta * -rvec.x;
                //f.z = cos_theta + one_minus_cos * rvec.z * rvec.z;

                //r.x = cos_theta + one_minus_cos * rvec.x * rvec.x;
                //r.y = one_minus_cos * rvec.x * rvec.y + sin_theta * rvec.z;
                //r.z = one_minus_cos * rvec.x * rvec.z + sin_theta * -rvec.y;

                //According to this link (http://stackoverflow.com/questions/36561593/opencv-rotation-rodrigues-and-translation-vectors-for-positioning-3d-object-in)
                //opencv and unity coordinate systems differ and y-axis should be flipped - from my own testing, this is not the case, so it is not done here
                //f.y = -f.y; //Reverse for opencv/unity coordinate system difference
                //u.y = -u.y;
                //r.y = -r.y;

                //Quaternion new_rot = Quaternion.LookRotation(f, u);
                
                    //Draw the rotation matrix vectors (which are up/right/foward of the given rotation) and rotation axis for debugging
                //Debug.DrawLine(tvec, tvec + f * 0.044f, Color.blue, 3);
                //Debug.DrawLine(tvec, tvec + u * 0.044f, Color.green, 3);
                //Debug.DrawLine(tvec, tvec + r * 0.044f, Color.red, 3);
                
                //Debug.DrawLine(tvec, tvec + rvec * 0.09f, Color.yellow, 3);
                //Debug.Log(theta * Mathf.Rad2Deg);
            }

            

            //Debug.Log(markers);
        //}
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
