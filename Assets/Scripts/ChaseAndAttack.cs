using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseAndAttack
{
    // class for handling agent chasing and attacking

    // -----------------------------
    // constants for alert levels
    private const int SEARCHING = 0;
    private const int CHASING = 1;
    private static readonly float[] maxTimeInLevel = { 10f, 3f };
    private const float randomPosGenerationRange = 5f;
    private const float meleeRange = 1.5f;
    public static readonly string[] playerAndPrisonerTags = { "Player", "Character" };
    public static readonly string[] onlyPlayerTag = { "Player" };
    public string[] tagsToLookFor = playerAndPrisonerTags;
    public Bounds? chaseIfSpottedWithin = null;

    // total fov, distance from center after which agent will attack, units agent can see
    public float fieldOfView = 175, attackingFieldOfView = 15, viewDistance = 10;
    public int numberOfRaysToCast = 50;
    // -----------------------------

    // the agent that is using this class
    public Character agent, chasing;

    // null for fists
    private Game.Items? weapon = null;
    private bool chaseOnSight = false;
    private float timeInCurrentLevel;
    private int level = -1;
    private Vector3 lastSeenPos, lastRandomPos;

    public void Update()
    {
        if (level == CHASING)
        {
            equipWeapon(true);
        }
        else
        {
            equipWeapon(false);
        }

        if (chaseOnSight || level != -1 || chaseIfSpottedWithin != null)
        {
            Tuple<RaycastHit2D, float> ray;

            if (level != CHASING)
            {
                if (tagsToLookFor != null)
                {
                    ray = AgentHelpers.lookForObjectWithTags(agent.gameObject, tagsToLookFor, agent.transform.position, agent.transform.up,
                        viewDistance, numberOfRaysToCast, fieldOfView);
                } else {
                    return;
                }
            }
            else
            {
                // look for character that we're chasing already
                ray = AgentHelpers.lookForObject(agent.gameObject, chasing.gameObject, agent.transform.position, agent.transform.up,
                    viewDistance, numberOfRaysToCast, fieldOfView);
            }

            if (agent.name.Contains("Guard") && ray != null)
            {
                Debug.Log(ray.Item1.collider.gameObject.name);
            }

            // we found object to chase (player or prisoner)
            if (ray != null &&
                ((agent.name.Contains("Guard") && !ray.Item1.collider.gameObject.name.Contains("Guard")) ||
                !agent.name.Contains("Guard")))
            {
                chasing = ray.Item1.collider.transform.gameObject.GetComponent<Character>();

                lastSeenPos = chasing.gameObject.transform.position;
                Debug.Log("last seen at " + lastRandomPos.ToString());

                if (level == CHASING || chaseOnSight || (chaseIfSpottedWithin != null &&
                    chaseIfSpottedWithin.Value.Contains(lastSeenPos)))
                {
                    Debug.Log("is within");
                    level = CHASING;

                    if (level == CHASING)
                    {
                        timeInCurrentLevel = 0;
                        if (Math.Abs(ray.Item2) < attackingFieldOfView)
                        {
                            Debug.Log(ray.Item2);
                            equipWeapon(true);
                            attack();
                            if (chasing.isDead())
                            {
                                level = -1;
                                timeInCurrentLevel = 0;
                            }
                        }
                    }
                }
            }

            if (level == SEARCHING)
            {
                if ((lastRandomPos - agent.transform.position).magnitude < 2f)
                {
                    lastRandomPos = AgentHelpers.generateRandomPos(lastSeenPos, randomPosGenerationRange, randomPosGenerationRange);
                }
            }

            if (level != -1)
            {
                timeInCurrentLevel += Time.deltaTime;
                if (timeInCurrentLevel > maxTimeInLevel[level])
                {
                    level--;
                    timeInCurrentLevel = 0;
                }
            }
        }
    }

    public bool isChasingOrSearching()
    {
        return level != -1;
    }

    public void setCharacterToChase(Character c)
    {
        chasing = c;
    }

    public void startChasing()
    {
        level = CHASING;
        timeInCurrentLevel = 0;
    }

    // true to enable chasing when this agent sees any character
    public void setChaseOnSight(bool chase)
    {
        chaseOnSight = chase;
    }

    public void setWeaponToAttackWith(Game.Items? weapon)
    {
        this.weapon = weapon;
    }

    public Vector3? getDest()
    {
        if (level == CHASING)
        {
            return lastSeenPos;
        }
        else if (level == SEARCHING)
        {
            return lastRandomPos;
        }
        else
        {
            return null;
        }
    }

    private void attack()
    {
        Debug.Log("attacking");
        // only melee if within melee range
        if (((weapon == null || weapon == Game.Items.TwoHandStone || weapon == Game.Items.Pickaxe) &&
            (lastSeenPos - agent.transform.position).magnitude < meleeRange) ||
            weapon == Game.Items.Pistol || weapon == Game.Items.Shotgun)
        {
            agent.useItem();
        }
    }

    private void equipWeapon(bool equip)
    {
        if (equip)
        {
            agent.equippedItem = weapon;
        }
        else
        {
            agent.equippedItem = null;
        }
    }
}