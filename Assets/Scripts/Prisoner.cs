using JetBrains.Annotations;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Prisoner : Character
{
    public float fieldOfView = 170, shootingFieldOfView = 40, viewDistance = 2;
    public int numberOfRaysToCast = 50;
    private float angryTimer = 5;
    [SerializeField] NavMeshAgent agent;
    public Bounds cell;
    public Vector3? randomPos;
    public bool reachedSpot = false;
    private Chasing chase = new Chasing();
    private GameObject player;
    private Vector3? dest;
    Game.Phase currentPhase;

    public override void OnStart()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        chase.setFirer(gameObject);
        chase.Start();
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        agent.updatePosition = false;
        agent.autoRepath = true;
        chase.setViewDistance(viewDistance);
    }

    public override void OnUpdate()
    {
        if(currentPhase != controller.phase)
        {
            randomPos = null;
        }
        chase.onUpdate();

        if (angry)
        {
            Debug.Log("IM ANGRY");

            chase.setOnSight(true);
            Debug.Log(chase.getOnSight());
        }
        Bounds? maybePhaseBounds = controller.getBoundsOfCurrentPhase();
        //Vector3? dest = (maybePhaseBounds != null && randomPos == null) ? maybePhaseBounds.Value.center : randomPos;

        if (chase.getOnSight())
        {
            angryTimer -= Time.deltaTime;
            Tuple<RaycastHit2D, float> ray = chase.getRay();
            if (ray != null)
            {
                equippedItem = Game.Items.TwoHandStone;
                if (Mathf.Abs(ray.Item2) <= shootingFieldOfView &&ray.Item1.distance <= 3)
                {
                    useItem();
                    Debug.Log("shooting");
                }
            }
            else if (chase.getLevel() > -1)
            {
                dest = chase.getPosition().Value;
                reachedSpot = false;
            }
            else
            {
                // we are idling
                equippedItem = null;
            }
            if (dest.Value != null)
            {
                agent.SetDestination(dest.Value);
            }
        }
        if(angryTimer <= 0)
        {
            angryTimer = 5;
            angry = false;
            chase.setOnSight(false);
            randomPos = null;
            equippedItem = null;
        }

        else
        {
            if (randomPos == null)
            {
                reachedSpot = false;
                currentPhase = controller.phase;
                if (controller.phase == Game.Phase.ReturnToCell ||
                controller.phase == Game.Phase.Nighttime)
                {
                    float xOffset = UnityEngine.Random.Range(-cell.extents.x, cell.extents.x);
                    float yOffset = UnityEngine.Random.Range(-cell.extents.y, cell.extents.y);
                    randomPos = new Vector3(cell.center.x + xOffset, cell.center.y + yOffset, 0);
                }
                else
                {
                    float xOffset = UnityEngine.Random.Range(-maybePhaseBounds.Value.extents.x, maybePhaseBounds.Value.extents.x);
                    float yOffset = UnityEngine.Random.Range(-maybePhaseBounds.Value.extents.y, maybePhaseBounds.Value.extents.y);
                    randomPos = new Vector3(maybePhaseBounds.Value.center.x + xOffset, maybePhaseBounds.Value.center.y + yOffset, 0);
                }
                agent.SetDestination(randomPos.Value);
            }
        }
        Vector3 dir = agent.steeringTarget - transform.position;
        //Debug.Log((transform.position - randomPos.Value).magnitude);
        if (!reachedSpot || angry)
        {
            nudgingOn = true;
            moveInDirection(new Vector2(dir.x, dir.y).normalized, false);

        }
        if ((transform.position - randomPos.Value).magnitude < 2f)
        {

            moveInDirection(Vector2.zero, false);
            //setMovementVelocity(Vector2.zero);
            reachedSpot = true;
        }
        agent.nextPosition = transform.position;

        // sometimes agent gets desynced when going through mine teleporter
        // this is supposed to fix it but i dont know if it actually works
        // TODO maybe find better fix for agent desync
        //checkForAgentDesync(agent);
    }

    public override void OnWalkedOverItem(GameObject item)
    {
    }

    public override void OnDeath()
    {
    }

    public override void OnTriggerEnterExtra(Collider2D col)
    {
    }
}
