using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Game : ScriptableObject
{
    // constants and other game-wide stuff

    public const float walkingSpeed = 5f;
    public const float runningSpeed = 10f;

    // seconds after which an agent is allowed to change direction
    public const float agentDirectionChangeLimit = 0.25f;
}
