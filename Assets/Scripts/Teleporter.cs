using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] BoxCollider2D destination;

    public void OnCollisionEnter2D(Collision2D col) {
        float destX = destination.bounds.center.x + Random.Range(-destination.bounds.extents.x, destination.bounds.extents.x);
        float destY = destination.bounds.center.y + Random.Range(-destination.bounds.extents.y, destination.bounds.extents.y);

        col.gameObject.transform.position = new Vector3(destX, destY, 0);
    }
}
