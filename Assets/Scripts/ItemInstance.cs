using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using Items = ItemInfo.Items;

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

    private Ammo _ammo;

    public Ammo ammo {
        get {
            return _ammo;
        }
        set {
            if (_info.item != Items.Pistol || _info.item != Items.Shotgun) {
                Debug.LogError("set ammo on ItemInstance of type " + _info.item.ToString());
            } else {
                _ammo = value;
            }
        }
    }
}
