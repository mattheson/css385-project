using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Guard : Character
{
    // if this is null the guard will get a patrol position from the GameController
    // otherwise we generate a random position within bounds
    public BoxCollider2D bounds;

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

    public override void OnUpdate()
    {
        Debug.Log(level);
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
            equippedItem = Items.Pistol;
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
                    Debug.Log("bruh");
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
            if (bounds)
            {
                if (randomPos == null || (transform.position - randomPos.Value).magnitude <= 2f)
                {
                    Debug.Log("bruh");
                    float xOffset = UnityEngine.Random.Range(-bounds.bounds.extents.x, bounds.bounds.extents.x);
                    float yOffset = UnityEngine.Random.Range(-bounds.bounds.extents.y, bounds.bounds.extents.y);
                    randomPos = new Vector3(bounds.transform.position.x + xOffset, bounds.transform.position.y + yOffset, 0);
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
        // agent.Warp(transform.position);
    }

    public override void OnWalkedOverItem(GameObject item)
    {
    }
    public override void OnDeath()
    {
        controller.freePatroller(this);
    }

    private Vector3 generateRandomPositionWithOffset(Vector3 center, float xOffsetRange, float yOffsetRange)
    {
        float xOffset = UnityEngine.Random.Range(-xOffsetRange, xOffsetRange);
        float yOffset = UnityEngine.Random.Range(-yOffsetRange, yOffsetRange);
        return new Vector3(center.x + xOffset, center.y + yOffset, 0);
    }
    public override void OnTriggerEnterExtra(Collider2D col)
    {
    }
}
