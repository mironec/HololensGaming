using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : MonoBehaviour
{

    private GameObject[] staticAffectedObjects;
    public GameManager gameManager;
    public float gravityStrength = 0.01f;

    void Start()
    {
        var rigidbodies = FindObjectsOfType<Rigidbody>();
        staticAffectedObjects = new GameObject[rigidbodies.Length];
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            staticAffectedObjects[i] = rigidbodies[i].gameObject;
        }
    }

    void FixedUpdate()
    {
        if (gameManager.isGamePaused()) return;
        foreach(var s in staticAffectedObjects) {
            Rigidbody rigidbody = s.GetComponent<Rigidbody>();
            Vector3 force = transform.position - rigidbody.transform.position;
            force /= (float)Math.Pow(force.magnitude, 3);
            force *= gravityStrength;
            rigidbody.AddForce(force, ForceMode.VelocityChange);
        }
    }
}
