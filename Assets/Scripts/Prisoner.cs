using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Prisoner : Character
{
    [SerializeField] NavMeshAgent agent;

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
