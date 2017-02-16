using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour {

    public float speedBoostStrength = 1.0f;

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
        rigidbody.velocity += transform.forward * speedBoostStrength;
        Debug.Log(other.gameObject + " Entered!");
    }
}
