using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToFloor : MonoBehaviour {

    public PlayfieldPlacer playfieldPlacer;

	void Start () {
        playfieldPlacer.onPlayfieldSelected += OnPlayfieldSelected;
	}
	
	void Update () {
		
	}

    void OnPlayfieldSelected() {
        float originalX = gameObject.transform.position.x;
        float originalZ = gameObject.transform.position.z;
        gameObject.transform.position = new Vector3(originalX, SurfaceMeshesToPlanes.Instance.FloorYPosition, originalZ);
        Collider coll = GetComponent<Collider>();
        float angle = 0.0f;
        float radius = 0.0f;
        while(!Physics.CheckBox(gameObject.transform.position, coll.bounds.extents*0.99f, gameObject.transform.rotation)) {
            gameObject.transform.position = new Vector3(originalX + Mathf.Sin(angle)*radius, SurfaceMeshesToPlanes.Instance.FloorYPosition, originalZ + Mathf.Cos(angle)*radius);
            if (radius == 0.0f || angle + coll.bounds.extents.magnitude / radius / radius > 2 * Mathf.PI)
            {
                radius += coll.bounds.extents.magnitude;
                angle = 0.0f;
            }
            else {
                angle += coll.bounds.extents.magnitude / radius / radius;
            }
        }
    }
}
