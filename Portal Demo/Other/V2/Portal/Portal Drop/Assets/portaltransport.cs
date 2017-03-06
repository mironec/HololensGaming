using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portaltransport : MonoBehaviour {

    public GameObject otherPortal;
    public GameObject ball;
    public bool isColliderEnable = true; 
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
     	
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag =="Player")
        {
            other.transform.position = otherPortal.transform.position + otherPortal.transform.forward*0.1f;
        }
    }

}
