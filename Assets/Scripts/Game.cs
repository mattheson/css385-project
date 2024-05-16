
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEngine;

public static class Game
{
    public static float walkingSpeed = 5f;
    public static float runningSpeed = 10f;

    // seconds after which an agent (guard, prisoner) is allowed to change direction
    public const float agentDirectionChangeLimit = 0.25f;

    // max distance you can melee something
    public const float meleeDistance = 0.5f;

    public const int pistolClipSize = 12;
    public const int shotgunCapacity = 8;
    public const int maxPistolClips = 5;
    public const int maxShotgunShells = 48;

    public const int bulletsSpawnedShotgun = 8;

    public const float minuteLengthInSeconds = 0.5f;

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
    public const float fistsForce = 5;
    public const float fistsForceDuration = 1;
    public const float pickaxeForce = 5;
    public const float pickaxeForceDuration = 0.5f;
    public const float pistolForce = 5;
    public const float pistolForceDuration = 0.25f;
    public const float twoHandStoneForce = 10;
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
        Mealtime = 0, Work = 1, FreeTime = 2, ReturnToCell = 3, Nighttime = 4
    }

    // string names for phases
    // order needs to be same
    public static readonly string[] PhaseNames = {
        "Breakfast",
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