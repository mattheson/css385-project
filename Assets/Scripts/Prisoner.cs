using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Prisoner : Character
{
    [SerializeField] NavMeshAgent agent;
    public Bounds cell;

    private Vector3 dest;
    private ChaseAndAttack chaseAndAttack;
    private bool registered = false, touchingStone = false;

    public override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        agent.updatePosition = false;
        agent.autoRepath = true;

        chaseAndAttack = new ChaseAndAttack
        {
            agent = this
        };

        float rand = Random.value;

        if (rand <= 0.5)
        {
            chaseAndAttack.setWeaponToAttackWith(null);
        }
        else if (rand > 0.5 && rand <= 0.75)
        {
            chaseAndAttack.setWeaponToAttackWith(Game.Items.TwoHandStone);
        }
        else if (rand > 0.75)
        {
            chaseAndAttack.setWeaponToAttackWith(Game.Items.Pickaxe);
        }
    }

    public void generateNewDestination()
    {
        if (controller.phase == Game.Phase.FreeTime)
        {
            dest = controller.findRandomTileInPhase(Game.Phase.FreeTime);
        }
        else if (controller.phase == Game.Phase.Work)
        {
            dest = controller.findRandomTileInPhase(Game.Phase.Work);
        }
        else if (controller.phase == Game.Phase.ReturnToCell)
        {
            dest = cell.center;
        }
    }

    public override void OnUpdate()
    {
        if (touchingStone)
        {
            equippedItem = Game.Items.Pickaxe;
            useItem();
            equippedItem = null;
        }

        if (!registered)
        {
            if (controller != null)
            {
                controller.phaseChanged.AddListener(generateNewDestination);
                registered = true;
                generateNewDestination();
            }
            else
            {
                return;
            }
        }

        Vector3? caDest = chaseAndAttack.getDest();

        if ((dest - transform.position).magnitude <= 2f)
        {
            if (controller.phase == Game.Phase.Work || controller.phase == Game.Phase.FreeTime)
            {
                generateNewDestination();
            }
            else if (controller.phase == Game.Phase.ReturnToCell || controller.phase == Game.Phase.Nighttime) {
                moveInDirection(Vector2.zero, false);
                return;
            }
        }

        agent.SetDestination(caDest != null ? caDest.Value : dest);

        chaseAndAttack.Update();

        agent.nextPosition = transform.position;

        moveInDirection((agent.steeringTarget - transform.position).normalized, false);

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
        controller.phaseChanged.RemoveListener(generateNewDestination);
    }

    public override void OnTriggerEnterExtra(Collider2D col)
    {
    }

    public override void OnHit(Character attacker)
    {
        chaseAndAttack.setCharacterToChase(attacker);
        chaseAndAttack.startChasing();
        setRandomPain();
        audioS.Play();
    }

    public override void OnCollisionEnter2DExtra(Collision2D col)
    {
        if (controller.phase == Game.Phase.Work && col.gameObject.name.Contains("Stone"))
        {
            touchingStone = true;
        }
    }
    public override void OnCollisionExit2DExtra(Collision2D col)
    {
        if (controller.phase == Game.Phase.Work && col.gameObject.name.Contains("Stone"))
        {
            touchingStone = false;
        }
    }
}
