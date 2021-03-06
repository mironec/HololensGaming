﻿using UnityEngine;
using HoloToolkit.Unity;

/// <summary>
/// GestureAction performs custom actions based on
/// which gesture is being performed.
/// </summary>
public class GestureAction : MonoBehaviour
{
    [Tooltip("Rotation max speed controls amount of rotation.")]
    public float RotationSensitivity = 10.0f;

    [Tooltip("Translation sensitivity controls the speed of the translation.")]
    public float TranslationSensitivity = 10.0f;

    [Tooltip("The main camera gameObject.")]
    public GameObject playerCamera;

    private bool doingTranslation = false;
    private bool doingRotation = false;
    private bool beingManipulated = false;
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
        transform.position = Vector3.zero;
        Vector3 scale = transform.localScale;
        transform.localScale = Vector3.one;
        Quaternion rotation = transform.rotation;
        transform.rotation = Quaternion.identity;

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        transform.GetComponent<MeshCollider>().sharedMesh = new Mesh();
        transform.GetComponent<MeshCollider>().sharedMesh.CombineMeshes(combine);
        gameObject.AddComponent<MeshFilter>();
        gameObject.GetComponent<MeshFilter>().sharedMesh = transform.GetComponent<MeshCollider>().sharedMesh;
        gameObject.SetActive(true);

        transform.position = initialPosition;
        transform.localScale = scale;
        transform.rotation = rotation;
    }

    void Awake()
    {
        foreach (Transform c in transform)
        {
            if (c.gameObject.GetComponent<GestureHelpResponder>() == null)
                c.gameObject.AddComponent<GestureHelpResponder>();
        }

        CustomGestureManager.Instance.OnManipulationStarted += PerformManipulationStart;
        CustomGestureManager.Instance.OnManipulationCompleted += PerformManipulationEnd;
        CustomGestureManager.Instance.OnManipulationCanceled += PerformManipulationEnd;
    }

    void OnSelect()
    {
        doingTranslation = true;
        doingRotation = false;
        /*isSelected = true;
        foreach(Transform c in transform)
        {
            c.gameObject.GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 1.0f);
        }*/
    }

    void OnDoubleTap()
    {
        Debug.Log("Double tap A.");
        doingRotation = true;
        doingTranslation = false;
    }

    void OnReleased()
    {
        /*isSelected = false;
        foreach (Transform c in transform)
        {
            c.gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f);
        }*/
    }

    private void snapToGrid() {
        transform.eulerAngles = new Vector3(roundAngle(transform.eulerAngles.x), roundAngle(transform.eulerAngles.y), roundAngle(transform.eulerAngles.z));
    }

    private float roundAngle(float angle)
    {
        angle = Mathf.Round(angle / 90.0f);
        if (angle == 4.0f) angle = 0.0f;
        if (angle == -4.0f) angle = 0.0f;

        return (angle * 90.0f);
    }

    void Update()
    {
        if (doingRotation)
            PerformRotation();
        if (doingTranslation)
            PerformTranslation();
    }

    private void PerformRotation()
    {
        if (/*IsThisFocusedByGaze() && */CustomGestureManager.Instance.ManipulationInProgress)
        {
            float rotationAroundY = CustomGestureManager.Instance.ManipulationOffset.x * RotationSensitivity;
            float rotationAroundCamera = CustomGestureManager.Instance.ManipulationOffset.y * RotationSensitivity;

            transform.Rotate(new Vector3(0.0f, -rotationAroundY, 0.0f), Space.World);
            transform.RotateAround(transform.position, Vector3.Cross(Vector3.up, playerCamera.transform.forward), rotationAroundCamera);
            Debug.Log("rotationAroundCamera: " + rotationAroundCamera);
            Debug.Log("playerCamera.forward: " + playerCamera.transform.forward);
        }
    }

    private bool IsThisFocusedByGaze() {
        return CustomGestureManager.Instance.FocusedObject != null && (CustomGestureManager.Instance.FocusedObject == gameObject || CustomGestureManager.Instance.FocusedObject.transform.parent.gameObject == gameObject);
    }

    public void PerformManipulationStart(UnityEngine.VR.WSA.Input.InteractionSourceKind sourceKind)
    {
        if (IsThisFocusedByGaze()) {
            initialPosition = transform.position;
            beingManipulated = true;
        }
    }

    public void PerformManipulationEnd(UnityEngine.VR.WSA.Input.InteractionSourceKind sourceKind)
    {
        beingManipulated = false;
        snapToGrid();
    }

    private void PerformTranslation()
    {
        if (beingManipulated)
        {
            transform.position = initialPosition + CustomGestureManager.Instance.ManipulationOffset * TranslationSensitivity;
            if (transform.position.y < SurfaceMeshesToPlanes.Instance.FloorYPosition)
                transform.position = new Vector3(transform.position.x, SurfaceMeshesToPlanes.Instance.FloorYPosition, transform.position.z);
            if (transform.position.y > SurfaceMeshesToPlanes.Instance.CeilingYPosition)
                transform.position = new Vector3(transform.position.x, SurfaceMeshesToPlanes.Instance.CeilingYPosition, transform.position.z);
        }
    }

    private class GestureHelpResponder : MonoBehaviour
    {
        void OnSelect()
        {
            transform.parent.SendMessage("OnSelect");
        }
        void OnReleased()
        {
            transform.parent.SendMessage("OnReleased");
        }
        void OnDoubleTap()
        {
            transform.parent.SendMessage("OnDoubleTap");
        }
    }
}