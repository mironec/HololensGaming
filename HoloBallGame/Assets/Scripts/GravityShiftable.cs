using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Rigidbody))]
public class GravityShiftable : MonoBehaviour {

    [HideInInspector]
    public Vector3 gravityDirection = Vector3.down;
    [HideInInspector]
    public float magnitude = Physics.gravity.magnitude;

	void Start () {
	}
	
	void FixedUpdate () {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (!rigidbody.useGravity) {
            rigidbody.AddForce(gravityDirection.normalized * magnitude * rigidbody.mass, ForceMode.Force);
        }
	}
}
