using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// base class to prevent method hiding of Unity functions
// DO NOT inherit from this, only Character should
public abstract class CharacterBase : MonoBehaviour
{
    public abstract void Start();
    public abstract void Update();
    public abstract void FixedUpdate();
}

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public abstract class Character : CharacterBase
{
    // this is an abstract class for all characters to inherit from
    // contains basic character functionality

    // --------------------------------------------------------------------
    // constants/offsets for spawning bullets, melee, etc.
    // offset is relative to character is facing upwards (towards +y)
    private readonly Vector2 pistolOffset = new Vector2(0.15f, 0.9f);
    private readonly Vector2 shotgunOffset = new Vector2(0.15f, 0.9f);
    private readonly Vector2 pickaxeOffset = new Vector2(0.15f, 0.9f);
    private readonly Vector2 rightPunchOffset = new Vector2(0.15f, 0.7f);
    private readonly Vector2 leftPunchOffset = new Vector2(-0.15f, 0.7f);
    private readonly Vector2 twoHandStoneOffset = new Vector2(0.0f, 0.7f);
    // --------------------------------------------------------------------

    [SerializeField] Sprite bodySprite;
    [SerializeField] CharacterAnimator animator;
    Rigidbody2D characterRigidbody;

    private Vector3 movementVel, agentLastPos, agentNudge;

    public Game.Items? equippedItem;
    public int health;

    // seconds to nudge after, magnitude of random velocity to apply,
    // threshold under which magnitude of diff of two positions is considered stuck
    private const float nudgeAfter = 1f, nudgeAmount = 10f, stuckThresh = 0.05f;

    // minimum number of seconds needed for agent to switch keypresses
    public const float agentNewKeyTime = 0.25f;

    // if agent is further than this away from character transform
    // agent will be warped
    public const float agentDesyncMagnitude = 10;

    private float agentSecsSinceLast, agentSecsStuck;

    protected GameController controller;

    private List<Tuple<float, float, Vector2>> hits = new List<Tuple<float, float, Vector2>>();

    // more agent unstuck stuff
    private const float maxTimeCollided = 1.5f;
    private const float totalNoclipTime = 1f;
    private bool isCollidingWithAgent = false;
    private float timeCollided = 0f;
    private bool isNoClipping = false;
    private float noClipTime = 0f;


    private float walkingSpeed, runningSpeed;
    //Audio Sources and sounds
    public AudioSource audioS;
    public AudioClip[] deathSounds = new AudioClip[3];
    public AudioClip[] painSounds = new AudioClip[8];
    public AudioClip[] Sounds;
    public AudioListener audioListener;
    private bool goingToDie = false;


    public sealed override void Start()
    {
        audioListener = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioListener>();
        walkingSpeed = Game.walkingSpeed;
        runningSpeed = Game.runningSpeed;
        characterRigidbody = GetComponent<Rigidbody2D>();
        GetComponent<SpriteRenderer>().sprite = bodySprite;
        animator.character = this;
        movementVel = Vector3.zero;
        health = 100;

        OnStart();
    }

    public sealed override void Update()
    {
        if (!characterRigidbody) characterRigidbody = GetComponent<Rigidbody2D>();

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
        }
        else
        {
            OnUpdate();
        }

        if (isCollidingWithAgent)
        {
            timeCollided += Time.deltaTime;
        }

        if (timeCollided > maxTimeCollided)
        {
            gameObject.layer = LayerMask.NameToLayer("CharacterIgnoreCollisions");
            isNoClipping = true;
        }

        if (isNoClipping)
        {
            noClipTime += Time.deltaTime;
        }
        if (noClipTime > totalNoclipTime)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            isNoClipping = false;
            noClipTime = 0;
            timeCollided = 0;
        }
    }

    public sealed override void FixedUpdate()
    {
        characterRigidbody.velocity = movementVel;
        for (int i = 0; i < hits.Count;)
        {
            (float remaining, float total, Vector2 force) = hits[i];
            remaining -= Time.fixedDeltaTime;
            if (remaining <= 0)
            {
                hits.RemoveAt(i);
                continue;
            }
            else
            {
                characterRigidbody.velocity += force * (remaining / total);
                // Debug.Log(characterRigidbody.velocity);
            }
            hits[i] = new Tuple<float, float, Vector2>(remaining, total, force);
            i++;
        }
        characterRigidbody.velocity += new Vector2(agentNudge.x, agentNudge.y);
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
        float speed = running ? runningSpeed : walkingSpeed;
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
        if (health == 0 || direction == Vector2.zero)
        {
            animator.stopWalking();
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
        else if (equippedItem == Game.Items.Pistol)
        {
            animator.shootPistol();
        }
        else if (equippedItem == Game.Items.Shotgun)
        {
            animator.shootShotgun();
        }
        else if (equippedItem == Game.Items.Pickaxe)
        {
            animator.swingPickaxe();
        }
        else if (equippedItem == Game.Items.TwoHandStone)
        {
            animator.swingTwoHandStone();
        }
    }

    // play reload animation
    public void reloadItem()
    {
        if (health == 0) return;
        if (equippedItem == Game.Items.Pistol)
        {
            animator.reloadPistol();
        }
        else if (equippedItem == Game.Items.Shotgun)
        {
            animator.loadShotgun();
        }
    }

    // just used to reset punch as of now
    public void resetPunch()
    {
        animator.resetPunch();
    }

    public void spawnPistolBullet()
    {
        Vector2 offset = transform.rotation * pistolOffset;
        controller.spawnPistolBullet(new Vector2(transform.position.x + offset.x, transform.position.y + offset.y),
            transform.up, this);
    }

    public void spawnShotgunShot()
    {
        Vector2 offset = transform.rotation * shotgunOffset;
        controller.spawnShotgunShot(new Vector2(transform.position.x + offset.x, transform.position.y + offset.y),
            transform.up, this);
    }

    public void rightPunchImpact()
    {
        punchImpact(rightPunchOffset);
    }

    public void leftPunchImpact()
    {
        punchImpact(leftPunchOffset);
    }

    // TODO ive added sleeping and item usage stuff in here
    private void punchImpact(Vector2 offset)
    {
        RaycastHit2D? ray = castMeleeRay(offset);
        if (ray != null)
        {
            if (ray.Value.collider.CompareTag("Character") || ray.Value.collider.CompareTag("Player"))
            {
                ray.Value.collider.GetComponent<Character>().hit(
                    CompareTag("Player") ? Game.playerFistsDamage : Game.agentFistsDamage,
                    transform.up,
                    Game.fistsForce,
                    Game.fistsForceDuration,
                    this
                );
            }

            // sleep if we punched bed
            if (CompareTag("Player") && ray.Value.collider.CompareTag("Bed"))
            {
                controller.sleep();
            }

            // increase player walking and running speed when punch fitness equipments
            if (ray.Value.collider.CompareTag("Fitness"))
            {
                increaseSpeed();
            }
        }
    }

    public void pickaxeImpact()
    {
        RaycastHit2D? ray = castMeleeRay(pickaxeOffset);
        if (ray != null)
        {
            if (ray.Value.collider.CompareTag("Breakable Walls"))
            {
                ray.Value.collider.GetComponent<BreakableWallTilemap>().pickaxeHit(
                    controller.worldToCell(ray.Value.point)
                );
            }
            else if (ray.Value.collider.CompareTag("Character") || ray.Value.collider.CompareTag("Player"))
            {
                ray.Value.collider.gameObject.GetComponent<Character>().hit(
                    CompareTag("Player") ? Game.playerPickaxeDamage : Game.agentPickaxeDamage,
                    transform.up,
                    Game.pickaxeForce,
                    Game.pickaxeForceDuration,
                    this
                );
            }
        }
    }

    public void twoHandStoneImpact()
    {
        RaycastHit2D? ray = castMeleeRay(twoHandStoneOffset);
        if (ray != null)
        {
            if (ray.Value.collider.CompareTag("Character") || ray.Value.collider.CompareTag("Player"))
            {
                ray.Value.collider.GetComponent<Character>().hit(
                    CompareTag("Player") ? Game.playerTwoHandStoneDamage : Game.agentTwoHandStoneDamage,
                    transform.up,
                    Game.twoHandStoneForce,
                    Game.twoHandStoneForceDuration,
                    this
                );
            }
        }
    }
    public void pistolReload()
    {
        Debug.Log("pistol reload");
    }

    public void shotgunLoad()
    {
        Debug.Log("shotgun load");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Item"))
        {
            OnWalkedOverItem(col.gameObject);
        }
        else
        {
            OnTriggerEnterExtra(col);
        }
    }

    public void hit(int damage, Vector2 incomingDirection, float force, float forceTime, Character firer)
    {
        hits.Add(new Tuple<float, float, Vector2>(forceTime, forceTime, incomingDirection * force));
        applyDamage(damage);

        OnHit(firer);
    }

    public void applyDamage(int damage)
    {
        if (health > 0)
        {
            health -= damage;
            if (health < 0) health = 0;
            if (health == 0)
            {
                goingToDie = true;
                OnDeath();
            }
        }
    }

    // returns gameobject hit by melee
    // if nothing hit returns null
    private RaycastHit2D? castMeleeRay(Vector2 offset)
    {
        offset = transform.rotation * offset;
        Vector2 origin = new Vector2(transform.position.x + offset.x,
            transform.position.y + offset.y);
        Vector2 direction = transform.up;
        Debug.DrawLine(origin, origin + (direction * Game.meleeDistance), Color.red, 5);
        RaycastHit2D[] rays = Physics2D.RaycastAll(origin, direction,
            Game.meleeDistance, ~LayerMask.GetMask("Ignore Raycast"));
        foreach (RaycastHit2D r in rays)
        {
            Debug.Log(r);
            if (!r.collider.gameObject.Equals(gameObject)) return r;
        }
        return null;
    }

    public void checkForAgentDesync(NavMeshAgent agent)
    {
        if ((agent.transform.position - transform.position).magnitude >= agentDesyncMagnitude)
        {
            agent.Warp(transform.position);
        }
    }

    public void increaseSpeed()
    {
        walkingSpeed += Game.fitnessSpeedIncreaseWalking;
        runningSpeed += Game.fitnessSpeedIncreaseRunning;
    }

    public bool isDead() {
        return health == 0;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // all characters with "Character" tag are assumed to be agents
        if (CompareTag("Character"))
        {
            if (col.gameObject.CompareTag("Character"))
            {
                isCollidingWithAgent = true;
            }
        }

        OnCollisionEnter2DExtra(col);
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (CompareTag("Character"))
        {
            if (col.gameObject.CompareTag("Character"))
            {
                isCollidingWithAgent = false;
                timeCollided = 0;
            }
        }
        OnCollisionExit2DExtra(col);
    }

    public void setRandomPain()
    {
        if (!goingToDie)
        {
            volumeCheck(transform.position, audioListener.GetComponentInParent<Transform>().position);
            audioS.clip = painSounds[UnityEngine.Random.Range(0, painSounds.Length)];
        }
        else
        {
            setRandomDeath();
        }
    }
    public void setRandomDeath()
    {
        volumeCheck(transform.position, audioListener.GetComponentInParent<Transform>().position);
        audioS.clip = deathSounds[UnityEngine.Random.Range(0, deathSounds.Length)];
    }
    public void volumeCheck(Vector3 soundOutput, Vector3 listenerPosition)
    {
        if(Vector3.Distance(listenerPosition, soundOutput) < 1)
        {
            audioS.volume = 1;
        }
        else if (Vector3.Distance(listenerPosition, soundOutput) < 15)
        {
            audioS.volume = 2 / Vector3.Distance(listenerPosition, soundOutput);
            Debug.Log(audioS.volume);
        }
        else
        {
            audioS.volume = 0;
        }
    }

    // Abstract functions
    public abstract void OnWalkedOverItem(GameObject item);

    // called in Start(), Update(), etc.
    // do not override Start(), Update(), override these functions
    public abstract void OnStart();
    public abstract void OnUpdate();
    public abstract void OnDeath();
    public abstract void OnHit(Character attacker);
    public abstract void OnTriggerEnterExtra(Collider2D col);
    public abstract void OnCollisionEnter2DExtra(Collision2D col);
    public abstract void OnCollisionExit2DExtra(Collision2D col);
}
