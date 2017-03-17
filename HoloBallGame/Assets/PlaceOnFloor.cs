using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceOnFloor : MonoBehaviour {

    public PlayfieldPlacer playfieldPlacer;


	void Start () {
        playfieldPlacer.onPlayfieldSelected += OnPlayfieldSelected;
	}

    private void OnDestroy()
    {
        playfieldPlacer.onPlayfieldSelected -= OnPlayfieldSelected;
    }

    private void OnPlayfieldSelected() {
        transform.position = new Vector3(transform.position.x, SurfaceMeshesToPlanes.Instance.FloorYPosition, transform.position.z);
    }
}
