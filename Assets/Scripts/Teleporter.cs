using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Teleporter : MonoBehaviour
{
    [SerializeField] BoxCollider2D destination;

    public void OnCollisionEnter2D(Collision2D col)
    {
        float destX = destination.bounds.center.x + Random.Range(-destination.bounds.extents.x, destination.bounds.extents.x);
        float destY = destination.bounds.center.y + Random.Range(-destination.bounds.extents.y, destination.bounds.extents.y);

        Vector3 telePos = new Vector3(destX, destY, 0);

        col.gameObject.transform.position = telePos;

        // if you dont warp agent stuff gets messed up
        NavMeshAgent agent = col.gameObject.GetComponent<NavMeshAgent>();

        if (agent)
        {
            Debug.Log("here");
            agent.Warp(telePos);
        }

        // move camera if it's the player
        if (col.gameObject.CompareTag("Player"))
        {
            Camera cam = FindFirstObjectByType<Camera>();
            if (cam != null)
            {
                cam.transform.position = new Vector3(telePos.x, telePos.y, cam.transform.position.z);
            }
        }
    }
}
