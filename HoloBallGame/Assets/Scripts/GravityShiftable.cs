using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Rigidbody))]
public class GravityShiftable : MonoBehaviour {

    public Vector3 gravityDirection = Vector3.down;
    public float magnitude = 9.81f;

	void Start () {
	}
	
	void FixedUpdate () {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (!rigidbody.useGravity) {
            rigidbody.AddForce(gravityDirection.normalized * magnitude * rigidbody.mass, ForceMode.Force);
        }
	}
}
