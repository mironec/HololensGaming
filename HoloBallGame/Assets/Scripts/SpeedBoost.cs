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
        if (speedBoostDirectionRelative.magnitude != 0)
        {
            direction = speedBoostDirectionRelative.normalized;
        }
        rigidbody.AddForce(direction * speedBoostStrength, ForceMode.VelocityChange);
    }
}
