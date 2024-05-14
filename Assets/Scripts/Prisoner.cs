using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Prisoner : Character
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform t;

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
        if (t) {
            agent.SetDestination(t.position);
        }
        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            Vector3 dir = agent.steeringTarget - transform.position;
            moveInDirection(new Vector2(dir.x, dir.y).normalized, false);
        }
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
