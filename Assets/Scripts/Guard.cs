using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class Guard : MonoBehaviour
{
    [SerializeField] GameObject target;
    [SerializeField] Sprite rightArmSprite;
    [SerializeField] Sprite leftArmSprite;

    private Character character;
    private CharacterAnimator anim;
    private NavMeshAgent agent;
    private bool4 keys;
    private float secsSince;
    void Start()
    {
        character = GetComponent<Character>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        agent.updatePosition = false;

        anim = transform.Find("Character Animations").GetComponent<CharacterAnimator>();
        anim.leftArmSprite = leftArmSprite;
        anim.rightArmSprite = rightArmSprite;
    }

    void Update()
    {
        agent.SetDestination(target.transform.position);
        if (agent.pathStatus == NavMeshPathStatus.PathComplete && secsSince >= Constants.agentDirectionChangeLimit)
        {
            Vector3 corner = agent.steeringTarget;
            Vector3 direction = corner - transform.position;
            Vector2 curr = new Vector2(direction.x, direction.y);
            curr = curr.normalized;
            keys = GameHelpers.convertDirectionToButtons(curr);
            secsSince = 0;
        }

        character.move(keys[0], keys[1], keys[2], keys[3], false);
        agent.nextPosition = transform.position;
        secsSince += Time.deltaTime;
    }
}
