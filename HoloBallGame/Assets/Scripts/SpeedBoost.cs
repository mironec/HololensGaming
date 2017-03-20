using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour {

    public float speedBoostStrength = 1.0f;
    public Vector3 speedBoostDirectionRelative;
    public bool removeVelocity = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rigidbody = other.gameObject.GetComponent<Rigidbody>();
        if (rigidbody == null) return;
        if(removeVelocity)
            rigidbody.velocity = Vector3.zero;
        Vector3 direction = transform.forward;
        if (speedBoostDirectionRelative != null) {
            direction = Quaternion.AngleAxis(speedBoostDirectionRelative.x, Vector3.right) * direction;
            direction = Quaternion.AngleAxis(speedBoostDirectionRelative.y, Vector3.up) * direction;
            direction = Quaternion.AngleAxis(speedBoostDirectionRelative.z, Vector3.forward) * direction;
        }
        rigidbody.AddForce(direction * speedBoostStrength, ForceMode.VelocityChange);
    }
}
