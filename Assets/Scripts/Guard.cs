using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Unity.Mathematics;
using UnityEditor.VersionControl;
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
    private Chasing chase = new Chasing();
    // -----------------------------
    // Used to tell if the guard is meant to be a boundedGuard
    private bool boundedGuard;
    public TilemapCollider2D office;

    public override void OnStart()
    {

        office = GameObject.FindGameObjectWithTag("office").GetComponent<TilemapCollider2D>();

        agent = GetComponent<NavMeshAgent>();
        chase.setFirer(gameObject);
        chase.Start();
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
        if (boundedGuard)
        {
            chase.setOnSight(true);
        }
        //Debug.Log(gameObject.name);
        chase.onUpdate();
        // the logic right now is just that guards follow paths nonstop
        // if nighttime and player spotted we chase them

        //chase.setOnSight(true); // true if the guard should chase/shoot player when it sees them 

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
            chase.setLevel(CHASING);
            chase.setOnSight(true);
        }

        else if (controller.phase == Game.Phase.Nighttime)
        {
            chase.setOnSight(true);
        }

        if (angry)
        {
            chase.setOnSight(true);
        }

        if (chase.getOnSight())
        {
            Tuple<RaycastHit2D, float> ray = 
            //AgentVision.lookForObject(gameObject, player,transform.position, transform.up,viewDistance, numberOfRaysToCast, fieldOfView);
            chase.getRay();
            // Debug.Log(ray);
            // we spotted player
            if (ray != null)
            {
                equippedItem = Game.Items.Pistol;
                if (Mathf.Abs(ray.Item2) <= shootingFieldOfView)
                {
                    useItem();
                    Debug.Log("shooting");
                }
                lastPlayerPosition = chase.getPosition().Value;
            }
            else if (chase.getLevel() > -1)
            {
                dest = chase.getPosition().Value;
            }
            else
            {
                // we are idling
                equippedItem = null;
            }
        } 
        else {

            // if not onsight we assume the guard is just patrolling
            equippedItem = null;
            chase.setLevel(-1);
            timeInCurrentLevel = 0;
        }

        // if guard is not chasing player just go to next position in path
        if (chase.getLevel() < 0) {
            if (!boundedGuard)
            {
                Vector3 currentPathPoint = path.points[pathIdx].position;
                if ((transform.position - currentPathPoint).magnitude <= 2f)
                {
                    pathIdx = (pathIdx + 1) % path.points.Count;
                }
                dest = currentPathPoint;
                //agent.SetDestination(dest);
            }
            else
            {
                if (randomPos == null)
                {
                    float xOffset = UnityEngine.Random.Range(-office.bounds.extents.x, office.bounds.extents.x);
                    float yOffset = UnityEngine.Random.Range(-office.bounds.extents.y, office.bounds.extents.y);
                    randomPos = new Vector3(office.bounds.center.x +xOffset, office.bounds.center.y + yOffset, 0);
                    dest = randomPos.Value;
                    Debug.Log("Spawned Random dest");
                    //agent.SetDestination(dest);
                }
                if((transform.position - dest).magnitude <= 2f){
                    Debug.Log("RandomPos reached");
                    randomPos = null;
                }
            }
        }
        if (boundedGuard)
        {
            Debug.Log(dest);
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
    public void setBoundedGuard(bool b) { boundedGuard = b; }
}
