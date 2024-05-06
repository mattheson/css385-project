using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class ItemInstance : MonoBehaviour
{
    private ItemInfo _info;
    public ItemInfo info {
        get {
            return _info;
        }
        set {
            _info = value;
            GetComponent<SpriteRenderer>().sprite = _info.groundSprite;
        }
    }

    // ammo if this is a gun
    public int? ammo;
}
