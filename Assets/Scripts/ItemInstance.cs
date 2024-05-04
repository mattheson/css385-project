using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInstance : MonoBehaviour
{
    private ItemInfo info;
    public ItemInfo Info {
        get {
            return info;
        }
        set {
            info = value;
            GetComponent<SpriteRenderer>().sprite = info.groundSprite;
        }
    }
}
