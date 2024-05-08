
using System.Collections.Generic;
using UnityEngine;

public static class Game
{
    public const float walkingSpeed = 5f;
    public const float runningSpeed = 10f;

    // seconds after which an agent is allowed to change direction
    public const float agentDirectionChangeLimit = 0.25f;

    public const float meleeDistance = 0.5f;

    public const int pistolClipSize = 12;
    public const int shotgunCapacity = 8;
    public const int maxPistolClips = 5;
    public const int maxShotgunShells = 48;
}

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
    Mealtime, Work, FreeTime, ReturnToCell, Nighttime
}

public enum CharacterTypes {
    Player, Prisoner, Guard, BoundedGuard
}