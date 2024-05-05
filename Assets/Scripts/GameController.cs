using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public class GameController : MonoBehaviour
{
    [SerializeField] Grid grid;
    [SerializeField] GameObject itemPrefab;

    // TODO this is the cleanest way of associating item info with an enum
    // that I have found so far, maybe there is something i'm missing
    // i wanted these things:
    // - an item enum
    // - a way to associate sprites and other object info with the enum
    // - have things be relatively easy to extend if we want to add more items
    // and a serializable dictionary seems like the best way to do this
    // ItemInfo is just a ScriptableObject stores the sprite/item info
    //
    // SerializedDictionary was downloaded from the asset store, it has an MIT license: 
    // https://github.com/ayellowpaper/SerializedDictionary
    [SerializeField]
    [SerializedDictionary("Item", "Item Info")]
    SerializedDictionary<ItemInfo.Items, ItemInfo> itemInfo;

    void Start()
    {

    }

    void Update()
    {

    }

    public void spawnItem(Vector2 pos, ItemInfo.Items item)
    {
        GameObject i = Instantiate(itemPrefab, grid.GetCellCenterWorld(grid.WorldToCell(new Vector3(pos.x, pos.y, 0))), Quaternion.identity);
        i.GetComponent<ItemInstance>().info = itemInfo[item];
    }
}
