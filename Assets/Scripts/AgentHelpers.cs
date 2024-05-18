
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.Dynamic;

public class AgentHelpers
{
    // casts rays, looks for object 
    // TODO this could be cleaner

    public static Tuple<RaycastHit2D, float> lookForObject(GameObject firer, GameObject target, Vector2 origin, Vector2 direction, float distance, int numRays, float fov)
    {
        if (numRays % 2 == 1)
        {
            RaycastHit2D? firstRay = castRayAndSearch(firer, target, origin, direction, distance);
            if (firstRay != null)
            {
                return new Tuple<RaycastHit2D, float>(firstRay.Value, 0);
            }
        }

        float angleIncrease = fov / numRays;
        float currentAngle = angleIncrease;

        int numLayers = (int)Math.Floor((float)numRays / 2);

        for (int layer = 1; layer <= numLayers; layer++)
        {
            RaycastHit2D? ray = castRayAndSearch(firer, target, origin, getVectorFromAngle(90 - currentAngle), distance);
            if (ray != null) return new Tuple<RaycastHit2D, float>(ray.Value, currentAngle);
            ray = castRayAndSearch(firer, target, origin, getVectorFromAngle(90 + currentAngle), distance);
            if (ray != null) return new Tuple<RaycastHit2D, float>(ray.Value, -currentAngle);
            currentAngle += angleIncrease;
        }

        return null;
    }

    public static Tuple<RaycastHit2D, float> lookForObjectWithTags(GameObject firer, string[] tags, Vector2 origin, Vector2 direction, float distance, int numRays, float fov)
    {
        if (numRays % 2 == 1)
        {
            RaycastHit2D? firstRay = castRayAndSearchForTags(firer, tags, origin, direction, distance);
            if (firstRay != null)
            {
                return new Tuple<RaycastHit2D, float>(firstRay.Value, 0);
            }
        }

        float angleIncrease = fov / numRays;
        float currentAngle = angleIncrease;

        int numLayers = (int)Math.Floor((float)numRays / 2);

        for (int layer = 1; layer <= numLayers; layer++)
        {
            RaycastHit2D? ray = castRayAndSearchForTags(firer, tags, origin, getVectorFromAngle(90 - currentAngle), distance);
            if (ray != null) return new Tuple<RaycastHit2D, float>(ray.Value, currentAngle);
            ray = castRayAndSearchForTags(firer, tags, origin, getVectorFromAngle(90 + currentAngle), distance);
            if (ray != null) return new Tuple<RaycastHit2D, float>(ray.Value, -currentAngle);
            currentAngle += angleIncrease;
        }

        return null;
    }

    private static RaycastHit2D? castRayAndSearch(GameObject firer, GameObject target, Vector2 origin, Vector2 direction, float distance)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, firer.transform.rotation * direction, distance, ~LayerMask.GetMask("Ignore Raycast"));
        Debug.DrawRay(origin, firer.transform.rotation * direction * distance, Color.red, 0.01f);

        foreach (RaycastHit2D h in hits)
        {
            if (!h.transform.gameObject.Equals(firer))
            {
                if (h.transform.gameObject.Equals(target)) return h;
                else return null;
            }
        }

        return null;

    }

    private static RaycastHit2D? castRayAndSearchForTags(GameObject firer, string[] tags, Vector2 origin, Vector2 direction, float distance)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, firer.transform.rotation * direction, distance, ~LayerMask.GetMask("Ignore Raycast"));
        Debug.DrawRay(origin, firer.transform.rotation * direction * distance, Color.red, 0.01f);

        foreach (RaycastHit2D h in hits)
        {
            if (!h.transform.gameObject.Equals(firer))
            {
                foreach (string s in tags)
                {
                    if (h.transform.gameObject.CompareTag(s)) return h;
                }
                return null;
            }
        }

        return null;
    }

    private static Vector2 getVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    // generates random position somewhere between origin - range to origin + range
    public static Vector3 generateRandomPos(Vector3 origin, float xRange, float yRange)
    {
        return new Vector3(
            origin.x + UnityEngine.Random.Range(-xRange, xRange),
            origin.y + UnityEngine.Random.Range(-yRange, yRange),
            0
        );
    }
}