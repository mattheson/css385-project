using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Prisoner : Character
{
    [SerializeField] NavMeshAgent agent;
    public Bounds cell;

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
        Bounds? maybePhaseBounds = controller.getBoundsOfCurrentPhase();
        Vector3 dest = maybePhaseBounds != null ? maybePhaseBounds.Value.center : transform.position;

        if (controller.phase == Game.Phase.ReturnToCell ||
            controller.phase == Game.Phase.Nighttime) {
            dest = cell.center;
        }

        agent.SetDestination(dest);

        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            Vector3 dir = agent.steeringTarget - transform.position;
            moveInDirection(new Vector2(dir.x, dir.y).normalized, false);
        }

        agent.nextPosition = transform.position;

        // sometimes agent gets desynced when going through mine teleporter
        // this is supposed to fix it but i dont know if it actually works
        // TODO maybe find better fix for agent desync
        checkForAgentDesync(agent);
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
