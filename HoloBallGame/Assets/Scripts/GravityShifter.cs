using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityShifter : MonoBehaviour {

	void Start () {
		
	}
	
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherGameObject = other.gameObject;
        GravityShiftable gravityShiftable = otherGameObject.GetComponent<GravityShiftable>();
        Rigidbody rigidbody = otherGameObject.GetComponent<Rigidbody>();
        if (gravityShiftable != null && rigidbody != null) {
            if (rigidbody.useGravity)
            {
                rigidbody.useGravity = false;
                gravityShiftable.gravityDirection = Vector3.up;
                gravityShiftable.magnitude = Physics.gravity.magnitude;
            }
            else {
                rigidbody.useGravity = true;
            }
        }
    }
}
