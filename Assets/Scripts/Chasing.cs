using AYellowpaper.SerializedCollections.Editor.Data;
using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements.Experimental;

public class Chasing : MonoBehaviour
{
    public float fieldOfView, shootingFieldOfView, viewDistance;
    public int numberOfRaysToCast;



    // -1 if agent is patrolling/idling
    private int level = -1;
    private float timeInCurrentLevel = 0;
    private Vector3? randomPos;
    private Vector3 lastPlayerPosition;
    private GameObject player;
    private bool onSight = false;


    private const int SEARCHING = 0;
    private const int CHASING = 1;
    private static readonly float[] maxTimeInLevel = { 10f, 3f };
    private const float randomPosGenerationRange = 5f;

    private Tuple<RaycastHit2D, float> ray;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    // Seperate out weapons
    // Current State
    // Enable chasing manually start chasing
    // Enable Chasing on vision 
    // Get position to chase too
    // All logic for finding player should be handeled here
    void Update()
    {
        ray = AgentVision.lookForObject(gameObject, player, transform.position, transform.up,
              viewDistance, numberOfRaysToCast, fieldOfView);

        // we spotted player
        if (onSight && ray != null)
        {
            level = CHASING;
            lastPlayerPosition = ray.Item1.transform.position;
            timeInCurrentLevel = 0;
        }
        else if (level > -1)
        {
            // we didnt spot player
            if (timeInCurrentLevel > maxTimeInLevel[level])
            {
                level--;
                timeInCurrentLevel = 0;
            }

            if (level == SEARCHING)
            {
                if (randomPos == null || (transform.position - randomPos.Value).magnitude <= 2f)
                {
                    float xOffset = UnityEngine.Random.Range(-randomPosGenerationRange, randomPosGenerationRange);
                    float yOffset = UnityEngine.Random.Range(-randomPosGenerationRange, randomPosGenerationRange);
                    randomPos = new Vector3(lastPlayerPosition.x + xOffset, lastPlayerPosition.y + yOffset, 0);
                }
            }
            else if (level == CHASING)
            {
            }
            timeInCurrentLevel += Time.deltaTime;
        }
    }
    public bool isChasing()
    {
        return level > -1;
    }
    public Vector3? getPosition()
    {
        if(level == SEARCHING)
        {
            return randomPos.Value;
        }
        if (isChasing())
        {
            return player.transform.position;
        }
        return null;
    }
    public Tuple<RaycastHit2D, float> getRay()
    {
        return ray;
    }
    public void setOnSight(bool s)
    {
        onSight = s;
    }
    public void startChasing()
    {
        level = CHASING;
    }

}
