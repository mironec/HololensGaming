using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using HoloToolkit.Sharing;

public class LocatorRunner : MonoBehaviour {
    public MeshRenderer CamTexRenderer; //assumed to be a standard plane
    public TextMesh debug_text;

    // Original Video parameters
    public int deviceNumber;

    private float frame_time = 0;
    private WebCamTexture _webcamTexture;
    private int cam_width;
    private int cam_height;

    private ImageTagLocationAdapter callbacks;
    private ImageTagManager tag_manager;

    private Dictionary<int, GameObject> tag_objects;
    private HashSet<int> prev_frame_tags;
    private HashSet<int> cur_frame_tags;

    private Vector3 plane_up;
    private Vector3 plane_right;

    private float half_width = 5;
    private float half_height = 5;
    
    
    // Use this for initialization
    void Start () {

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {

            _webcamTexture = new WebCamTexture(devices[deviceNumber].name, 1280/2, 720/2, 30);
            
            // Play the video source
            CamTexRenderer.material.mainTexture = _webcamTexture;
            _webcamTexture.Play();

            cam_width = _webcamTexture.width;
            cam_height = _webcamTexture.height;
            Debug.Log(cam_width);
            Debug.Log(cam_height);   
        }
        else
        {
            Debug.Log("Couldn't find a webcam!");
        }

        callbacks = new ImageTagLocationAdapter();
        callbacks.LocatedEvent += OnTagLocated;
        callbacks.CompletedEvent += OnLocatorComplete;

        tag_manager = ImageTagManager.Create();

        tag_objects = new Dictionary<int, GameObject>();
        prev_frame_tags = new HashSet<int>();
        cur_frame_tags = new HashSet<int>();

        plane_up = new Vector3(0, 0, 1); //up for the unrotated plane
        plane_right = new Vector3(1, 0, 0);
        plane_up = gameObject.transform.rotation * plane_up; //Rotate to match the placed plane
        plane_right = gameObject.transform.rotation * plane_right;

        half_width *= gameObject.transform.localScale.x;
        half_height *= -gameObject.transform.localScale.z;
    }

    void OnTagLocated(ImageTagLocation location)
    {
        //Debug.Log("Found tag");
        //Debug.Log(location.GetPixelX(ImageTagLocationType.Center));
        //Debug.Log(location.GetPixelY(ImageTagLocationType.Center));
        
        int id = location.GetTagId();

        Vector2 tag_pos = new Vector2();

        tag_pos.x = location.GetPixelX(ImageTagLocationType.Center);
        tag_pos.y = location.GetPixelY(ImageTagLocationType.Center);

        tag_pos.x /= cam_width;
        tag_pos.y /= cam_height;

        tag_pos *= -2; //negate it, since we're looking at the image effectively rotated 180 degrees (the image left ends up on the right of the plane)
        tag_pos.x += 1;
        tag_pos.y += 1;

        Vector3 virtual_pos = plane_up * half_height * tag_pos.y + plane_right * half_width * tag_pos.x;
        virtual_pos += gameObject.transform.position;

        Vector2 top_left = new Vector2();
        Vector2 top_right = new Vector2();

        GameObject obj;
        if(tag_objects.TryGetValue(id, out obj))
        {
            obj.transform.position = virtual_pos;
        }
        else
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cube); //instantiate obj here
            obj.transform.position = virtual_pos;
            tag_objects.Add(id, obj);
        }

        cur_frame_tags.Add(id);
        prev_frame_tags.Remove(id);

        //Debug.Log(frame_time);
    }

    void OnLocatorComplete()
    {
        frame_time = 0;

        foreach(int id  in prev_frame_tags)
        {
            GameObject obj;
            tag_objects.TryGetValue(id, out obj);
            if(obj != null)
            {
                Destroy(obj);
            }
            tag_objects.Remove(id);
        }
         //Make the current frame tags the previous one, and use the cleared previous as for collecting the next tags
        prev_frame_tags.Clear();
        HashSet<int> tmp = cur_frame_tags;
        cur_frame_tags = prev_frame_tags;
        prev_frame_tags = tmp;
    }
	
	// Update is called once per frame
	void Update () {
        tag_manager.Update();

        if (_webcamTexture.isPlaying)
        {
            if (_webcamTexture.didUpdateThisFrame)
            {
                Color32[] colors = _webcamTexture.GetPixels32();
                byte[] im_bytes = Color32ArrayToByteArray(colors);

                //Reverse 
                int bytes_per_row = cam_width * 4; //4 bytes per pixel
                byte[] temp_row = new byte[bytes_per_row];
                for (int i = 0; i < cam_height / 2; i++)
                {
                    int top_offset = i * bytes_per_row;
                    int bottom_offset = (cam_height - 1 - i) * bytes_per_row;
                    Array.Copy(im_bytes, top_offset, temp_row, 0, bytes_per_row);
                    Array.Copy(im_bytes, bottom_offset, im_bytes, top_offset, bytes_per_row);
                    Array.Copy(temp_row, 0, im_bytes, bottom_offset, bytes_per_row);

                }

                tag_manager.FindTags(im_bytes, cam_width, cam_height, 4, callbacks);
                frame_time += Time.deltaTime;
            }
        }
    }

    private byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        if (colors == null || colors.Length == 0)
            return null;

        int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
        int length = lengthOfColor32 * colors.Length;
        byte[] bytes = new byte[length];

        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(ptr, bytes, 0, length);
        }
        finally
        {
            if (handle != default(GCHandle))
                handle.Free();
        }

        return bytes;
    }
}
