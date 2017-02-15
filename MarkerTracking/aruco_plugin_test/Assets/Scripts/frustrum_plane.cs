using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class frustrum_plane : MonoBehaviour {

        //Vertical FOV 
    public float fov;
        //Camera aspect ratio
    public float aspect;

	// Use this for initialization
	void Start () {
        update_size();
    }

    public void update_size()
    {
        float distance = transform.localPosition.z;
        float frustrum_height = 2.0f * distance * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float frustrum_width = frustrum_height * aspect;

        Vector2 local_scale = transform.localScale;

        local_scale.x = frustrum_width;
        local_scale.y = frustrum_height;

        transform.localScale = local_scale;
    }
}
