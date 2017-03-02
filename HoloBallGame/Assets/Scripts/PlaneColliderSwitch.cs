using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneColliderSwitch : MonoBehaviour
{

    Collider[] colliding;

    void Start()
    {
        colliding = Physics.OverlapBox(GetComponent<Collider>().bounds.center, GetComponent<Collider>().bounds.extents / 2);
    }

    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (var c in colliding)
        {
            if(c == GetComponent<Collider>())
            c.enabled = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        foreach (var c in colliding)
        {
            c.enabled = true;
        }
    }
}
