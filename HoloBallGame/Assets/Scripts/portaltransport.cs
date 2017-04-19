using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portaltransport : MonoBehaviour {

    public GameObject otherPortal;
    public bool isColliderEnable;

    // Use this for initialization
    void Start () { 

		
	}
	
	// Update is called once per frame
	void Update () {
     	
	}

    private void OnTriggerEnter(Collider other)
    {
        if (isColliderEnable == true)
        {
            otherPortal.GetComponent<portaltransport>().isColliderEnable = false;
            Vector3 relative_pos = other.transform.position - transform.position;
            other.transform.position = otherPortal.transform.position + relative_pos;
            Rigidbody rigidbody = other.GetComponent<Rigidbody>();
            Vector3 velocity = rigidbody.velocity;
            velocity *= -1;
            velocity = Quaternion.Inverse(transform.rotation) * velocity;
            velocity = otherPortal.transform.rotation * velocity;
            rigidbody.velocity = velocity;
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isColliderEnable = true;
    }

}
