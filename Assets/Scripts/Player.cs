using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float walkingSpeed = 5f;
    [SerializeField] float runningSpeed = 10f;
    [SerializeField] float lerpSpeed = 0.25f; // seconds to fully turn or something
    private Rigidbody2D rb;
    private float lerpTime = 0f;
    private Quaternion start, end;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        Vector3 vel = new Vector3();
        bool keyPressed = false;
        if (Input.GetKey(KeyCode.W))
        {
            vel.y += 1;
            keyPressed = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            vel.x -= 1;
            keyPressed = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            vel.y -= 1;
            keyPressed = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            vel.x += 1;
            keyPressed = true;
        }
        bool running = Input.GetKey(KeyCode.LeftShift);
        float speed = running ? runningSpeed : walkingSpeed;
        vel = vel.normalized * speed;
        rb.velocity = vel;

        if (keyPressed && !Vector3.zero.Equals(vel) && lerpTime < lerpSpeed) {
            if (lerpTime == 0) {
                start = transform.rotation;
                end = Quaternion.LookRotation(Vector3.forward, vel);
            }
            transform.rotation = Quaternion.Lerp(start, end, lerpTime += Time.fixedDeltaTime / (lerpSpeed / speed));
        } else {
            lerpTime = 0;
        }
    }
}
