using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemInfo : ScriptableObject
{
    // this is just a container for basic item info (sprites and enum)
    // put instance-specific stuff like ammo in ItemInstance

    public Items item;
    public ItemEquippability equippability;
    public Sprite groundSprite;
}
