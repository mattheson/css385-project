using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private FOV fieldOfView;
    public float speed = 15.0f;
    Vector2 movement = Vector2.zero;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate() {
        rb.velocity = movement * speed;
    }

    void OnMove(InputValue value) {
        movement = value.Get<Vector2>();
    }
}