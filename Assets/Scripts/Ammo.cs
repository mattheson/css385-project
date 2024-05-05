using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using Items = ItemInfo.Items;

public class Ammo
{
    public int reserve, mag;
    public static Ammo Zero() {
        Ammo ammo = new Ammo();
        ammo.reserve = 0;
        ammo.mag = 0;
        return ammo;
    }
}
