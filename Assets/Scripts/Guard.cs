using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Analytics;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(NavMeshAgent))]
public class Guard : Character
{
    public GameController.GuardPath path;
    private int pathIdx = 0;

    // TODO add bounded guards
    // (guards in front office)
    // guards all follow paths for now
    // public Bounds bounds;

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
        // the logic right now is just that guards follow paths nonstop
        // if nighttime and player spotted we chase them

        bool onSight = false; // true if the guard should chase/shoot player when it sees them 

        // make sure we have player
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

        if (path == null) return;

        Vector3 dest = transform.position;

        // chase and attack player if they failed to collect required gold
        // basically we want game over if they fail to get gold
        // and this is a dramatic way of doing that
        // might be better to just turn on onSight or something
        if (controller.playerFailedToCollectGold())
        {
            level = CHASING;
            onSight = true;
        }

        else if (controller.phase == Game.Phase.Nighttime)
        {
            onSight = true;
        }

        if (onSight)
        {
            Tuple<RaycastHit2D, float> ray = AgentVision.lookForObject(gameObject, player, transform.position, transform.up,
                viewDistance, numberOfRaysToCast, fieldOfView);

            // we spotted player
            if (ray != null)
            {
                equippedItem = Game.Items.Pistol;
                if (Mathf.Abs(ray.Item2) <= shootingFieldOfView)
                {
                    useItem();
                    Debug.Log("shooting");
                }
                level = CHASING;
                lastPlayerPosition = ray.Item1.transform.position;
                timeInCurrentLevel = 0;
                dest = player.transform.position;
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
                }
                else if (level == CHASING)
                {
                    agent.SetDestination(player.transform.position);
                }
                timeInCurrentLevel += Time.deltaTime;
            }
            else
            {
                // we are idling
                equippedItem = null;
            }
        } else {
            // if not onsight we assume the guard is just patrolling
            equippedItem = null;
            level = -1;
            timeInCurrentLevel = 0;
        }

        // if guard is not chasing player just go to next position in path
        if (level == -1) {
            Vector3 currentPathPoint = path.points[pathIdx].position;
            if ((transform.position - currentPathPoint).magnitude <= 2f) {
                pathIdx = (pathIdx + 1) % path.points.Count;
            }
            dest = currentPathPoint;
        }

        agent.SetDestination(dest);
        agent.nextPosition = transform.position;

        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            Vector3 dir = agent.steeringTarget - transform.position;
            moveInDirection(new Vector2(dir.x, dir.y).normalized, false);
        }
    }
    public override void OnWalkedOverItem(GameObject item)
    {
    }
    public override void OnDeath()
    {
        // controller.freePatroller(this);
    }

    public override void OnTriggerEnterExtra(Collider2D col)
    {
    }
}
