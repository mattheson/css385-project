using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

// base class to prevent method hiding of Unity functions
// DO NOT inherit from this, only Character should
public abstract class CharacterBase : MonoBehaviour {
    public abstract void Start();
    public abstract void Update();
    public abstract void FixedUpdate();
}

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public abstract class Character : CharacterBase 
{
    // this is an abstract class for all characters to inherit from
    // contains basic character functionality

    [SerializeField] Sprite bodySprite;
    [SerializeField] CharacterAnimator animator;
    [SerializeField] Rigidbody2D charaterRigidbody;

    // bullet spawning offset
    private readonly Vector2 pistolBulletOffset = new Vector2(0.15f, 0.9f);
    private Vector3 movementVel, agentLastPos, agentNudge;

    public Items? equippedItem;
    public int health;

    // seconds to nudge after, magnitude of random velocity to apply,
    // threshold under which magnitude of diff of two positions is considered stuck
    private const float nudgeAfter = 1f, nudgeAmount = 10f, stuckThresh = 0.05f;

    // minimum number of seconds needed for agent to switch keypresses
    public const float agentNewKeyTime = 0.25f;

    private float agentSecsSinceLast, agentSecsStuck;

    protected GameController controller;

    public sealed override void Start()
    {
        GetComponent<SpriteRenderer>().sprite = bodySprite;
        animator.character = this;
        movementVel = Vector3.zero;
        health = 100;

        OnStart();
    }

    public sealed override void Update()
    {
        // TODO remove this, just for demo
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.LoadScene("Game");
        }
        // TODO clean up these assignment statements
        // i found that some assignment statements didn't work in Start(),
        // and i didn't want to use inspector references which resulted in this
        // probably best to just use inspector references though
        if (!controller)
        {
            controller = FindFirstObjectByType<GameController>();
        }

        if (animator.character != this)
        {
            animator.character = this;
        }

        if (health == 0)
        {
            equippedItem = null;
            movementVel = Vector2.zero;
            animator.clearAllAnimations();
            GetComponent<SpriteRenderer>().material = controller.getDeadCharacterMaterial();
        } else {
            OnUpdate();
        }
    }

    public sealed override void FixedUpdate()
    {
        charaterRigidbody.velocity = movementVel;
        charaterRigidbody.velocity += new Vector2(agentNudge.x, agentNudge.y);
    }

    // all character movement happens here
    public void move(bool up, bool down, bool left, bool right, bool running)
    {
        if (animator.isSwingingTwoHandStone() || health == 0)
        {
            movementVel = Vector3.zero;
            animator.stopWalking();
            return;
        }

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
        movementVel = curr;

        if (moved)
        {
            animator.startWalking();
            transform.rotation = Quaternion.LookRotation(Vector3.forward, curr);
        }
        else
        {
            animator.stopWalking();
        }
    }

    // move an agent in the given direction
    // simulates player keypresses but will nudge if agent gets stuck 
    public void moveInDirection(Vector2 direction, bool running)
    {
        if (health == 0) {
            Debug.Log("agent");
            movementVel = Vector2.zero;
            return;
        }

        if (agentSecsSinceLast > agentNewKeyTime)
        {
            bool4 keys = GameHelpers.convertDirectionToButtons(direction);
            move(keys[0], keys[1], keys[2], keys[3], running);
            agentSecsSinceLast = 0;
        }
        else
        {
            agentSecsSinceLast += Time.deltaTime;
        }

        // check if guard is stuck
        // if so, apply a 'nudge' which just applies random velocity
        // TODO this might not be good
        if ((agentLastPos - transform.position).magnitude <= stuckThresh)
        {
            agentSecsStuck += Time.deltaTime;
            if (agentSecsStuck > nudgeAfter)
            {
                Debug.Log("nudging agent");
                agentNudge = new Vector2((UnityEngine.Random.value * 2) - 1, (UnityEngine.Random.value * 2) - 1).normalized * nudgeAmount;
            }
        }
        else
        {
            agentLastPos = transform.position;
            agentSecsStuck = 0f;
            agentNudge = Vector3.zero;
        }
    }

    // use equipped item
    public void useItem()
    {
        if (health == 0) return;
        if (equippedItem == null)
        {
            animator.punch();
        }
        else if (equippedItem == Items.Pistol)
        {
            animator.shootPistol();
        }
        else if (equippedItem == Items.Pickaxe)
        {
            animator.swingPickaxe();
        }
        else if (equippedItem == Items.TwoHandStone)
        {
            animator.swingTwoHandStone();
        }
    }

    // play reload animation
    public void reloadItem()
    {
        if (health == 0) return;
        if (equippedItem == Items.Pistol)
        {
            animator.reloadPistol();
        }
        else if (equippedItem == Items.Shotgun)
        {

        }
    }

    // just used to reset punch as of now
    public void idleItem()
    {
        animator.resetPunch();
    }

    public void spawnPistolBullet()
    {
        Vector2 offset = transform.rotation * pistolBulletOffset;
        controller.spawnBullet(new Vector2(transform.position.x + offset.x, transform.position.y + offset.y),
            transform.up, this);
    }

    public void punchImpact()
    {
        Debug.Log("punch");
    }

    public void pickaxeImpact()
    {
        controller.playerSwungPickaxe(transform.position, transform.up);
    }

    public void twoHandStoneImpact()
    {
        Debug.Log("stone");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Item"))
        {
            OnWalkedOverItem(col.gameObject);
        } else {
            OnTriggerEnterExtra(col);
        }
    }

    public void hitByBullet(int damage, Vector2 incomingDirection, float force, Character firer)
    {
        charaterRigidbody.velocity += incomingDirection * force;
        applyDamage(damage);
        Debug.Log(health);
    }

    public void applyDamage(int damage) {
        if (health > 0) {
            health -= damage;
            if (health < 0) health = 0;
            if (health == 0) {
                OnDeath();
            }
        }
    }

    // Abstract functions
    public abstract void OnWalkedOverItem(GameObject item);

    // called in Start(), Update(), etc.
    // do not override Start(), Update(), override these functions
    public abstract void OnStart();
    public abstract void OnUpdate();
    public abstract void OnDeath();

    // TODO maybe remove this, added this for demo
    public abstract void OnTriggerEnterExtra(Collider2D col);
}
