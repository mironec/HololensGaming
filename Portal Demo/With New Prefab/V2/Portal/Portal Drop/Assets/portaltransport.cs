using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portaltransport : MonoBehaviour {

    public GameObject otherPortal;
    public GameObject ball;
    public bool isColliderEnable;
    public bool enabled;

    // Use this for initialization
    void Start () { 

		
	}
	
	// Update is called once per frame
	void Update () {
     	
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && isColliderEnable == true)
        {
            isColliderEnable = false;
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

}
