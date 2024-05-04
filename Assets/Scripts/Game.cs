using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Game : ScriptableObject
{
    public const float walkingSpeed = 5f;
    public const float runningSpeed = 10f;

    // seconds for agent to change direction
    public const float agentDirectionChangeLimit = 0.25f;
}
