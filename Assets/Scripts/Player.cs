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
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {
        rb.velocity = movement * speed;
    }

    void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
        if (movement.x != 0 || movement.y != 0)
        {
            animator.SetFloat("X", movement.x);
            animator.SetFloat("Y", movement.y);

            animator.SetBool("isWalking", true);

        }
        else
        {
            animator.SetBool("isWalking", false);
        }

    }
}