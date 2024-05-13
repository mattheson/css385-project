using System.Collections;
using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D))]
public class Door : MonoBehaviour
{
    public float doorOpeningSpeed;
    public Transform doorOpenCenterPosition;
    public Transform doorClosedCenterPosition;
    public bool isOpen;

    private Rigidbody2D rb;
    private NavMeshObstacle ob;

    void Start() {
    }

    void FixedUpdate()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        Vector3 dist = (isOpen ? doorOpenCenterPosition.position : doorClosedCenterPosition.position) - transform.position;
        if (dist.magnitude <= 0.01) return;
        dist = dist.normalized;
        rb.MovePosition(transform.position + (dist * doorOpeningSpeed * Time.fixedDeltaTime));
    }

    public void toggleDoor()
    {
        isOpen = !isOpen;
    }
}
