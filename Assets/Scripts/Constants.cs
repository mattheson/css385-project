using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : MonoBehaviour
{
    public static float walkingSpeed = 5f;
    public static float runningSpeed = 10f;
    
    // seconds for agent to change direction
    public static float agentDirectionChangeLimit = 0.25f;
    public enum Items {
        Fists,
        Rock,
        Pickaxe,
        Pistol,
        Shotgun,
    }
}
