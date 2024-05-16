using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Prisoner : Character
{
    [SerializeField] NavMeshAgent agent;
    public Bounds cell;
    public Vector3? randomPos;
    public bool reachedSpot = false;

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
        //Vector3? dest = (maybePhaseBounds != null && randomPos == null) ? maybePhaseBounds.Value.center : randomPos;

        //if (controller.phase == Game.Phase.ReturnToCell ||
        //    controller.phase == Game.Phase.Nighttime) {
        //    dest = cell.center;
        //}
        if (randomPos == null)
        {

            float xOffset = UnityEngine.Random.Range(-maybePhaseBounds.Value.extents.x, maybePhaseBounds.Value.extents.x);
            float yOffset = UnityEngine.Random.Range(-maybePhaseBounds.Value.extents.y, maybePhaseBounds.Value.extents.y);
            randomPos = new Vector3(maybePhaseBounds.Value.center.x + xOffset, maybePhaseBounds.Value.center.y + yOffset, 0);
            agent.SetDestination(randomPos.Value);
        }

            Vector3 dir = agent.steeringTarget - transform.position;
            //Debug.Log((transform.position - randomPos.Value).magnitude);
            if (!reachedSpot)
            {
            if (Input.GetKey(KeyCode.Backspace))
            {
                moveInDirection(new Vector2(dir.x, dir.y).normalized, false);
            }
            else {
                moveInDirection(Vector2.zero, false);
            }
                
            }
            if((transform.position - randomPos.Value).magnitude < 2f)
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
