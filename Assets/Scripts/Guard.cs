using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(NavMeshAgent))]
public class Guard : Character
{
    // set `path` if you want a guard that will follow path throughout the day
    // set `bounds` if you want a guard that will stay within bounds
    public GameController.GuardPath path;
    public Bounds bounds;

    // fov when raycasting, fov for agent to shoot, distance agent can see
    public float fieldOfView, shootingFieldOfView, viewDistance;
    public int numberOfRaysToCast;

    private NavMeshAgent agent;

    // -1 if agent is patrolling/idling
    private int level = -1;
    private float timeInCurrentLevel = 0;
    private Vector3? randomPos;
    private Vector3 lastPlayerPosition;
    private GameObject player;

    // -----------------------------
    // constants for alert levels
    private const int SEARCHING = 0;
    private const int CHASING = 1;
    private static readonly float[] maxTimeInLevel = { 10f, 3f };
    private const float randomPosGenerationRange = 5f;
    // -----------------------------

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateUpAxis = false;
        agent.updateRotation = false;
        agent.updatePosition = false;
        agent.autoRepath = true;

        if (shootingFieldOfView >= fieldOfView)
        {
            Debug.LogError("need shooting fov to be less than full fov");
        }
    }

    // old logic
    /*
    public override void OnUpdate()
    {
        if (!player)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            if (players.Length == 0)
            {
                Debug.LogError("no Player found");
                return;
            }

            if (players.Length > 1)
            {
                Debug.LogError("multiple Players found, targeting first");
            }

            player = players[0];
        }

        Tuple<RaycastHit2D, float> ray = AgentVision.lookForObject(gameObject, player, transform.position, transform.up,
            viewDistance, numberOfRaysToCast, fieldOfView);

        if (ray != null)
        {
            // we spotted player
            equippedItem = Game.Items.Pistol;
            if (Mathf.Abs(ray.Item2) <= shootingFieldOfView)
            {
                useItem();
                Debug.Log("shooting");
            }
            level = CHASING;
            lastPlayerPosition = ray.Item1.transform.position;
            timeInCurrentLevel = 0;
            randomPos = null;
            agent.SetDestination(player.transform.position);
        }
        else if (level > -1)
        {
            // we didnt spot player
            if (timeInCurrentLevel > maxTimeInLevel[level])
            {
                level--;
                timeInCurrentLevel = 0;
            }

            if (level == SEARCHING)
            {
                if (randomPos == null || (transform.position - randomPos.Value).magnitude <= 2f)
                {
                    float xOffset = UnityEngine.Random.Range(-randomPosGenerationRange, randomPosGenerationRange);
                    float yOffset = UnityEngine.Random.Range(-randomPosGenerationRange, randomPosGenerationRange);
                    randomPos = new Vector3(lastPlayerPosition.x + xOffset, lastPlayerPosition.y + yOffset, 0);
                }
                agent.SetDestination(randomPos.Value);
            } else if (level == CHASING) {
                agent.SetDestination(player.transform.position);
            }
            timeInCurrentLevel += Time.deltaTime;
        }
        else
        {
            // we are idling
            equippedItem = null;
            if (bounds == null)
            {
                if (randomPos == null || (transform.position - randomPos.Value).magnitude <= 2f)
                {
                    float xOffset = UnityEngine.Random.Range(-bounds.extents.x, bounds.extents.x);
                    float yOffset = UnityEngine.Random.Range(-bounds.extents.y, bounds.extents.y);
                    randomPos = new Vector3(bounds.center.x + xOffset, bounds.center.y + yOffset, 0);
                }
                agent.SetDestination(randomPos.Value);
            }
            else
            {
                agent.SetDestination(controller.getNextPatrolPosition(this));
            }
        }

        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            Vector3 dir = agent.steeringTarget - transform.position;
            moveInDirection(new Vector2(dir.x, dir.y).normalized, false);
        }
        agent.nextPosition = transform.position;
    }
    */

    public override void OnUpdate()
    {

    }
    public override void OnWalkedOverItem(GameObject item)
    {
    }
    public override void OnDeath()
    {
        controller.freePatroller(this);
    }

    public override void OnTriggerEnterExtra(Collider2D col)
    {
    }
}
