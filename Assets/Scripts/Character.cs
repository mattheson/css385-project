using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public abstract class Character : MonoBehaviour
{
    // this is an abstract class for all characters to inherit from
    // contains basic character functionality

    [SerializeField] Sprite bodySprite, leftArmSprite, rightArmSprite, hairSprite;
    [SerializeField] CharacterAnimator animator;

    private Vector3 vel, agentLastPos, agentNudge;

    // seconds to nudge after, amount of steeringtarget to nudge by,
    // threshold under which magnitude of diff of two positions is considered stuck
    private const float nudgeAfter = 2f, nudgeAmount = 0.01f, stuckThresh = 0.05f;
    public const float agentNewKeyTime = 0.25f;
    private float agentSecsSinceLast, agentSecsStuck;

    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = bodySprite;
        animator.leftArmSprite = leftArmSprite;
        animator.rightArmSprite = rightArmSprite;
        animator.hairSprite = hairSprite;
        animator.character = this;
        vel = Vector3.zero;
    }

    void FixedUpdate()
    {
        GetComponent<Rigidbody2D>().velocity = vel;
        transform.position += agentNudge;
        agentNudge = Vector3.zero;
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

    // move an agent in the given direction
    // simulates player keypresses but will nudge if agent gets stuck 
    public void moveAgent(Vector2 direction, bool running) {
        if (agentSecsSinceLast > agentNewKeyTime) {
            bool4 keys = GameHelpers.convertDirectionToButtons(direction);
            move(keys[0], keys[1], keys[2], keys[3], running);
            agentSecsSinceLast = 0;
        } else {
            agentSecsSinceLast += Time.deltaTime;
        }

        // check if guard is stuck
        // if so, apply a 'nudge' which just applies random direction a tiny amount
        // TODO this might not be good
        if ((agentLastPos - transform.position).magnitude <= stuckThresh) {
            agentSecsStuck += Time.deltaTime;
            if (agentSecsStuck > nudgeAfter) {
                Debug.Log("nudging agent");
                agentNudge = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value).normalized * nudgeAmount; 
            }
        } else {
            agentLastPos = transform.position;
            agentSecsStuck = 0f;
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
