
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public static class Game
{
    public const float walkingSpeed = 5f;
    public const float runningSpeed = 10f;
    public const float fitnessSpeedIncreaseWalking = 1f;
    public const float fitnessSpeedIncreaseRunning = 1f;
    public const float maxWalkingSpeed = 10f;
    public const float maxRunningSpeed = 15f;

    // seconds after which an agent (guard, prisoner) is allowed to change direction
    public const float agentDirectionChangeLimit = 0.10f;

    // max distance you can melee something
    public const float meleeDistance = 0.5f;

    public const int pistolClipSize = 12;
    public const int shotgunCapacity = 8;
    public const int maxPistolClips = 5;
    public const int maxShotgunShells = 48;

    public const int bulletsSpawnedShotgun = 8;

    public const float minuteLengthInSeconds = 0.25f;

    // ---------------------------------------------------------
    // weapon damage constants
    // player... = damage that player applies using the weapon
    // agent... = damage that agent applies to player using the weapon
    public const int playerFistsDamage = 10;
    public const int agentFistsDamage = 5;
    public const int playerPickaxeDamage = 25;
    public const int agentPickaxeDamage = 10;
    public const int playerPistolDamage = 34;
    public const int agentPistolDamage = 15;
    public const int playerTwoHandStoneDamage = 100;
    public const int agentTwoHandStoneDamage = 80;
    // ---------------------------------------------------------

    // ---------------------------------------------------------
    // weapon force constants
    // (force applied when weapon hits character)
    public const float fistsForce = 15;
    public const float fistsForceDuration = 0.5f;
    public const float pickaxeForce = 10;
    public const float pickaxeForceDuration = 1.5f;
    public const float pistolForce = 5;
    public const float pistolForceDuration = 0.25f;
    public const float twoHandStoneForce = 20;
    public const float twoHandStoneForceDuration = 1;
    // ---------------------------------------------------------

    // items you can pick up off the ground
    public enum Items
    {
        Gold, TwoHandStone, Pickaxe, Pistol, Shotgun, MasterKey, Key
    }
    public enum ItemEquippability
    {
        SingleHand, DoubleHand, NotEquippable
    }

    public enum Phase
    {
        Work = 0, FreeTime = 1, ReturnToCell = 2, Nighttime = 3
    }

    // string names for phases
    // order needs to be same
    public static readonly string[] PhaseNames = {
        "Work",
        "Free Time",
        "Return to Cell",
        "Night Time"
    };

    public enum CharacterTypes
    {
        Player, Prisoner, Guard, BoundedGuard
    }
};