using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemInfo : ScriptableObject
{
    // this is just a container for item info
    public enum Items
    {
        Gold, Rock, Pickaxe, Pistol, Shotgun
    }

    public enum ItemEquippability
    {
        SingleHand, DoubleHand, NotEquippable
    }

    public Items item;
    public ItemEquippability equippability;
    public Sprite groundSprite;
    public Sprite idleHoldingSprite;
}
