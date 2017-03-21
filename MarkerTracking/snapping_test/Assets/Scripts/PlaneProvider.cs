using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneProvider : MonoBehaviour {

    public PlaneInfo[] planes;
	// Use this for initialization
	void Awake () {
        planes = new PlaneInfo[transform.childCount];
        int i = 0;
		foreach(Transform child in transform) {
            PlaneInfo newPlane = new PlaneInfo();
            newPlane.origin = child.position;
            newPlane.rotation = child.rotation;
            newPlane.inverseRotation = Quaternion.Inverse(child.rotation);
            newPlane.normal = child.rotation * Vector3.up;

            Vector2 size = new Vector2();
            size.x = child.localScale.x * 10;
            size.y = child.localScale.y * 10;
            newPlane.size = size;

            planes[i] = newPlane;
            i++;
        }
	}
}

public struct PlaneInfo {
    public Vector3 origin;
    public Vector3 normal;
    public Quaternion rotation;
    public Quaternion inverseRotation;
    public Vector2 size;
}