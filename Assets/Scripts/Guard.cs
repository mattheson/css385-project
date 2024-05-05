using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class Guard : Character 
{
    [SerializeField] GameObject target;

    private NavMeshAgent agent;

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
        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            Vector3 dir = agent.steeringTarget - transform.position;
            moveAgent(new Vector2(dir.x, dir.y).normalized, false);
        }

        agent.nextPosition = transform.position;
    }

    public override void OnWalkedOverItem(GameObject item)
    {
    }
}
