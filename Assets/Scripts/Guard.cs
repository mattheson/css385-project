using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class Guard : Character 
{
    [SerializeField] GameObject target;

    private NavMeshAgent agent;
    private bool4 keys;
    private float secsSince;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        agent.updatePosition = false;
        agent.autoRepath = true;
    }

    void Update()
    {
        agent.SetDestination(target.transform.position);
        if (agent.pathStatus == NavMeshPathStatus.PathComplete && secsSince >= Game.agentDirectionChangeLimit)
        {
            Vector3 corner = agent.steeringTarget;
            Vector3 direction = corner - transform.position;
            Vector2 curr = new Vector2(direction.x, direction.y);
            curr = curr.normalized;
            keys = GameHelpers.convertDirectionToButtons(curr);
            secsSince = 0;
        }

        move(keys[0], keys[1], keys[2], keys[3], false);
        agent.nextPosition = transform.position;
        secsSince += Time.deltaTime;
    }

    public override void OnWalkedOverItem(GameObject item)
    {
    }
}
