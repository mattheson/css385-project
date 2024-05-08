
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.Dynamic;

public class AgentVision
{
    // casts rays, looks for object 
    // returns the hit and the offset from 90 degrees where the object was hit
    public static Tuple<RaycastHit2D, float> lookForObject(GameObject firer, GameObject target, Vector2 origin, Vector2 direction, float distance, int numRays, float fov) {
        if (numRays % 2 == 1) {
            RaycastHit2D? firstRay = castRayAndSearch(firer, target, origin, direction, distance);
            if (firstRay != null) {
                return new Tuple<RaycastHit2D, float>(firstRay.Value, 0);
            }
        } 

        float angleIncrease = fov / numRays;
        float currentAngle = angleIncrease;

        int numLayers = (int) Math.Floor((float) numRays / 2);

        for (int layer = 1; layer <= numLayers; layer++) {
            RaycastHit2D? ray = castRayAndSearch(firer, target, origin, getVectorFromAngle(90 - currentAngle), distance);
            if (ray != null) return new Tuple<RaycastHit2D, float>(ray.Value, currentAngle);
            ray = castRayAndSearch(firer, target, origin, getVectorFromAngle(90 + currentAngle), distance);
            if (ray != null) return new Tuple<RaycastHit2D, float>(ray.Value, -currentAngle);
            currentAngle += angleIncrease;
        }

        return null;
    }

    private static RaycastHit2D? castRayAndSearch(GameObject firer, GameObject target, Vector2 origin, Vector2 direction, float distance) {
        // important note: 2 corresponds to the "ignore raycast" layer
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, distance, ~(1 << 2));
        Debug.DrawRay(origin, firer.transform.rotation * direction * distance, Color.red, 0.01f);

        foreach (RaycastHit2D h in hits) {
            if (!h.transform.gameObject.Equals(firer)) {
                Debug.Log(h.transform.name);
                if (h.transform.gameObject.Equals(target)) return h;
                else return null;
            }
        }

        return null;
    }

    // public float fov;
    // public int numberOfRaysToCast;
    // public float viewDistance;
    // public GameObject agent;

    // private Dictionary<GameObject, Tuple<RaycastHit2D, float>> objHits;
    // private Dictionary<string, List<GameObject>> tagHits;

    // direction is the center of the FOV
    // we start at center - fov / 2 to and go to center + fov / 2
    // TODO performance is not good for this for now
    // public void castRays(Vector2 origin, Vector2 direction)
    // {
    //     objHits.Clear();

    //     float currentAngle = -(fov / 2);
    //     float angleIncrease = fov / numberOfRaysToCast;

    //     Vector2 currentDirection = direction - getVectorFromAngle(fov / 2);
    //     Vector2 directionIncrease = getVectorFromAngle(angleIncrease);

    //     for (int ray = 0; ray < numberOfRaysToCast; ray++)
    //     {
    //         RaycastHit2D[] hits = Physics2D.RaycastAll(origin, currentDirection, viewDistance);
    //         RaycastHit2D? hit = null;
    //         foreach (RaycastHit2D h in hits)
    //         {
    //             if (!h.transform.gameObject.Equals(agent))
    //             {
    //                 hit = h;
    //                 break;
    //             }
    //         }
    //         if (hit != null)
    //         {
    //             GameObject obj = hit.Value.transform.gameObject;
    //             if (!objHits.ContainsKey(obj) || (Math.Abs((fov / 2) - currentAngle) < Math.Abs((fov / 2) - objHits[obj].Item2)))
    //             {
    //                 objHits[obj] = new Tuple<RaycastHit2D, float>(hit.Value, currentAngle);
    //             }
    //         }
    //         currentAngle += angleIncrease;
    //         currentDirection += directionIncrease;
    //     }
    // }

    // returns of ray that hit object
    // the float is the degree from the center where object was hit
    // null if object was not hit
    // public Tuple<RaycastHit2D, float> objectHitInfo(GameObject obj)
    // {
    //     return objHits.GetValueOrDefault(obj, null);
    // }

    // takes tag to look for
    // returns list of gameobjects with tags that were hit
    // use objectHitInfo to get info about each hit
    // public List<GameObject> tagHitInfo(string tag)
    // {

    // }

    private static Vector2 getVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
}