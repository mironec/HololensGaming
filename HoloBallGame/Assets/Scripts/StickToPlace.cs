using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToPlace : MonoBehaviour {

    public PlayfieldPlacer playfieldPlacer;
    public StickOptions stickOption = StickOptions.StickToFloor;
    public enum StickOptions {
        StickToFloor,
        StickToCeiling
    }

	void Start () {
        playfieldPlacer.onPlayfieldSelected += OnPlayfieldSelected;
	}

    private void OnDestroy()
    {
        if(playfieldPlacer != null)
            playfieldPlacer.onPlayfieldSelected -= OnPlayfieldSelected;
    }

    void Update () {
		
	}

    void OnPlayfieldSelected() {
        if (!enabled) return;
        float originalX = gameObject.transform.position.x;
        float originalZ = gameObject.transform.position.z;
        float y = 0.0f;
        if (stickOption == StickOptions.StickToFloor) y = SurfaceMeshesToPlanes.Instance.FloorYPosition;
        if (stickOption == StickOptions.StickToCeiling) y = SurfaceMeshesToPlanes.Instance.CeilingYPosition;
        gameObject.transform.position = new Vector3(originalX, y, originalZ);
        Collider coll = GetComponent<Collider>();
        float angle = 0.0f;
        float radius = 0.0f;
        while(!Physics.CheckBox(gameObject.transform.position, coll.bounds.extents*0.99f, gameObject.transform.rotation)) {
            gameObject.transform.position = new Vector3(originalX + Mathf.Sin(angle)*radius, y, originalZ + Mathf.Cos(angle)*radius);
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
