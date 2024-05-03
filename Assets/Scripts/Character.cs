using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Character : MonoBehaviour
{
    // this class contains shared character behavior (movement, animation, actions)
    // shared between player, guards, prisoners

    // all characters are assumed to have a child instance of "Character Animations" prefab
    private GameObject characterAnimations;
    private CharacterAnimator anim;
    private Vector3 vel;
    private List<Constants.Items> inv;

    void Start()
    {
        characterAnimations = transform.Find("Character Animations").gameObject;
        anim = characterAnimations.GetComponent<CharacterAnimator>();
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
        float speed = running ? Constants.runningSpeed : Constants.walkingSpeed;
        curr = curr.normalized * speed;
        vel = curr;

        if (moved) {
            anim.startWalking();
            transform.rotation = Quaternion.LookRotation(Vector3.forward, curr);
        } else {
            anim.stopWalking();
        }

        // if (moved && !Vector3.zero.Equals(vel) && lerpTime < lerpSpeed) {
        //     if (lerpTime == 0) {
        //         start = transform.rotation;
        //         end = Quaternion.LookRotation(Vector3.forward, vel);
        //     }
        //     transform.rotation = Quaternion.Lerp(start, end, lerpTime += Time.fixedDeltaTime / (lerpSpeed / speed));
        // } else {
        //     lerpTime = 0;
        // }
    }
}
