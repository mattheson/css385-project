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
    public bool boundedToOffice;
    private Vector3? officeRandPos = null;
    private int pathIdx = 0;

    // TODO add bounded guards
    // (guards in front office)
    // guards all follow paths for now
    // public Bounds bounds;

    // fov when raycasting, fov for agent to shoot, distance agent can see
    public float fieldOfView, shootingFieldOfView, viewDistance;
    public int numberOfRaysToCast;

    private NavMeshAgent agent;

    private ChaseAndAttack chaseAndAttack;

    public override void OnStart()
    {
        controller = FindFirstObjectByType<GameController>();

        agent = GetComponent<NavMeshAgent>();

        agent.updateUpAxis = false;
        agent.updateRotation = false;
        agent.updatePosition = false;
        agent.autoRepath = true;

        if (shootingFieldOfView >= fieldOfView)
        {
            Debug.LogError("need shooting fov to be less than full fov");
        }

        chaseAndAttack = new ChaseAndAttack
        {
            agent = this,
        };

        if (boundedToOffice)
        {
            chaseAndAttack.chaseIfSpottedWithin = controller.getOfficeBounds();
        }

        float rand = UnityEngine.Random.value;

        if (rand >= 0.25)
        {
            chaseAndAttack.setWeaponToAttackWith(Game.Items.Pistol);
        }
        else
        {
            chaseAndAttack.setWeaponToAttackWith(Game.Items.Shotgun);
        }
    }

    public override void OnUpdate()
    {
        if (controller.playerFailedToCollectGold() && controller.phase != Game.Phase.Nighttime)
        {
            chaseAndAttack.tagsToLookFor = ChaseAndAttack.onlyPlayerTag;
            chaseAndAttack.setChaseOnSight(true);
        }
        else if (controller.phase == Game.Phase.Nighttime)
        {
            chaseAndAttack.tagsToLookFor = ChaseAndAttack.playerAndPrisonerTags;
            chaseAndAttack.setChaseOnSight(true);
        }
        else
        {
            chaseAndAttack.setChaseOnSight(false);
        }

        Vector3 dest;
        Vector3? caDest = chaseAndAttack.getDest();

        if (caDest != null)
        {
            dest = caDest.Value;
        }
        else
        {
            if (boundedToOffice && officeRandPos == null)
            {
                officeRandPos = controller.findRandomTileInOffice();
            }

            // if guard is not chasing player just go to next position in path
            // or random pos in bounds
            dest = boundedToOffice ? officeRandPos.Value : path.points[pathIdx].position;

            if ((transform.position - dest).magnitude <= 2f)
            {
                if (!boundedToOffice)
                {
                    pathIdx = (pathIdx + 1) % path.points.Count;
                }
                else
                {
                    officeRandPos = controller.findRandomTileInOffice();
                }
            }
        }

        chaseAndAttack.Update();

        agent.SetDestination(dest);
        agent.nextPosition = transform.position;

        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            Vector3 dir = agent.steeringTarget - transform.position;
            moveInDirection(new Vector2(dir.x, dir.y).normalized, false);
        }
    }

    public override void OnHit(Character attacker)
    {
        chaseAndAttack.setCharacterToChase(attacker);
        chaseAndAttack.startChasing();
    }

    public override void OnWalkedOverItem(GameObject item)
    {
    }
    public override void OnDeath()
    {
        controller.guardDied(transform.position);
    }

    public override void OnTriggerEnterExtra(Collider2D col)
    {
    }

    public override void OnCollisionEnter2DExtra(Collision2D col)
    {
    }

    public override void OnCollisionExit2DExtra(Collision2D col) { }
}
