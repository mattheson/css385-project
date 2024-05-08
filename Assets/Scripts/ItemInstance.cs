using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInstance : MonoBehaviour
{
    public ItemInfo info = null;

    void Update() {
        if (info) GetComponent<SpriteRenderer>().sprite = info.groundSprite;
    }

    // ammo if this is a gun
    public int ammo = 0;
}
