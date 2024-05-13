using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // TODO this probably can be cleaner
    public float bulletSpeed;
    public float bulletForce;
    public Character firer;

    // number of updates for the bullet to stay around after reaching target
    public int numberOfUpdatesToLag;
    public float secondsToLiveAfterHit;
    private float secondsAfterHit;
    private GameObject hit;
    private float distanceToTravel, distanceTravelled;
    private Vector2 bulletDirectionWithSpeed;
    private Vector2 point;
    private bool startedMoving = false;
    private bool hitDest = false;

    public void startBullet()
    {
        RaycastHit2D[] rays = Physics2D.RaycastAll(transform.position, transform.up);
        foreach (RaycastHit2D r in rays)
        {
            if (!r.transform.gameObject.Equals(firer.gameObject))
            {
                hit = r.transform.gameObject;
                point = r.point;
                Vector2 diff = point - (Vector2) transform.position;
                bulletDirectionWithSpeed = diff.normalized * bulletSpeed;
                distanceToTravel = diff.magnitude;
                distanceTravelled = 0;
                startedMoving = true;
                break;
            }
        }
    }

    public void FixedUpdate()
    {
        if (!startedMoving) return;

        if (hitDest) {
            secondsAfterHit += Time.fixedDeltaTime;
            if (secondsAfterHit >= secondsToLiveAfterHit) {
                Destroy(gameObject);
            }
            return;
        }

        Vector2 maybeSmallDistanceLeft = point - (Vector2) transform.position;
        Vector2 movementThisUpdate = bulletDirectionWithSpeed * Time.fixedDeltaTime;
        Vector2 movement;

        if (maybeSmallDistanceLeft.magnitude < bulletSpeed * Time.fixedDeltaTime) {
            movement = maybeSmallDistanceLeft;
        } else {
            movement = movementThisUpdate;
        }

        transform.position += (Vector3) movement;
        distanceTravelled += movement.magnitude;

        if (Math.Abs(distanceToTravel - distanceTravelled) <= 0.005) {
            Character maybeCharacter = hit.GetComponent<Character>();
            if (maybeCharacter) {
                maybeCharacter.hit(34, transform.up, Game.pistolForce, firer);
            }
            hitDest = true;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(point, 1);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1);
    }
}
