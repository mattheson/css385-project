using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    private GameObject player;
    public float smooth = 1f;
    private Vector3 vel = Vector3.zero;

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 target = player.transform.position;
            target.z = transform.position.z;
            transform.position = Vector3.SmoothDamp(transform.position, target, ref vel, smooth);
        }
        else
        {
            player = GameObject.Find("Player");
        }
    }
}
