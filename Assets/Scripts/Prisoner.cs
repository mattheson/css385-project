using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Prisoner : Character 
{
    // Bounds for Prisoners reliant on the time of day
    public BoxCollider2D bounds;
    public BoxCollider2D cell;
    private NavMeshAgent agent;
    private Vector3? randomPos;

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateUpAxis = false;
        agent.updateRotation = false;
        agent.updatePosition = false;
        agent.autoRepath = true;
    }
    public override void OnUpdate()
    {
        switch(controller.getCurrentPhase())
        {
            case Phase.Work:
                bounds = GameObject.Find("Mine").GetComponent<BoxCollider2D>();
                break;
            case Phase.FreeTime:
                bounds = GameObject.Find("Freetime").GetComponent<BoxCollider2D>();
                break;
            case Phase.Mealtime:
                bounds = GameObject.Find("Cafeteria").GetComponent<BoxCollider2D>();
                break;
            case Phase.Nighttime:
            case Phase.ReturnToCell:
            default:
                bounds = cell;
                break;


        }
        if (bounds)
        {
            //TODO: Make this reliant on the schedule so that they return to where they are supposed to at that time
            if (randomPos == null || (transform.position - randomPos.Value).magnitude <= 2f)
            {
                float xOffset = UnityEngine.Random.Range(-bounds.bounds.extents.x, bounds.bounds.extents.x);
                float yOffset = UnityEngine.Random.Range(-bounds.bounds.extents.y, bounds.bounds.extents.y);
                randomPos = new Vector3(bounds.transform.position.x + xOffset, bounds.transform.position.y + yOffset, 0);
            }
            agent.SetDestination(randomPos.Value);
        }
        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            Vector3 dir = agent.steeringTarget - transform.position;
            moveInDirection(new Vector2(dir.x, dir.y).normalized, false);
        }
        //agent.Warp(transform.position);
        agent.nextPosition = transform.position;

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
