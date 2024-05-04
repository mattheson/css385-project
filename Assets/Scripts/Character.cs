using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class Character : MonoBehaviour
{
    // this is an abstract class for all characters to inherit from
    // contains basic character functionality

    [SerializeField] Sprite bodySprite;
    [SerializeField] Sprite leftArmSprite;
    [SerializeField] Sprite rightArmSprite;
    [SerializeField] CharacterAnimator animator;

    private Vector3 vel;

    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = bodySprite;
        animator.leftArmSprite = leftArmSprite;
        animator.rightArmSprite = rightArmSprite;
        vel = Vector3.zero;
    }

    void FixedUpdate()
    {
        GetComponent<Rigidbody2D>().velocity = vel;
    }

    // all character movement happens here
    public void move(bool up, bool down, bool left, bool right, bool running) {
        Vector3 curr = new Vector3();
        bool moved = false;
        if (up)
        {
            curr.y += 1;
            moved = true;
        }
        if (left)
        {
            curr.x -= 1;
            moved = true;
        }
        if (down)
        {
            curr.y -= 1;
            moved = true;
        }
        if (right)
        {
            curr.x += 1;
            moved = true;
        }
        float speed = running ? Game.runningSpeed : Game.walkingSpeed;
        curr = curr.normalized * speed;
        vel = curr;

        if (moved) {
            animator.startWalking();
            transform.rotation = Quaternion.LookRotation(Vector3.forward, curr);
        } else {
            animator.stopWalking();
        }
    }

    public void punch() {
        animator.punch();
    }

    public void resetPunch() {
        animator.resetPunch();
    }

    public abstract void OnWalkedOverItem(GameObject item);
}
