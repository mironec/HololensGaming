using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour {

    public float speedBoostStrength = 1.0f;
    public Vector3 speedBoostDirectionAbsolute;
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
        if (speedBoostDirectionAbsolute.magnitude != 0)
        {
            direction = speedBoostDirectionAbsolute.normalized;
        }
        if (speedBoostDirectionRelative.magnitude != 0)
        {
            direction = transform.rotation * Quaternion.Euler(speedBoostDirectionRelative) * Vector3.forward;
        }
        rigidbody.AddForce(direction * speedBoostStrength, ForceMode.VelocityChange);
    }
}
