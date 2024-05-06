using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public class GameController : MonoBehaviour
{
    [SerializeField] Grid grid;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] GameObject bulletPrefab;

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
    SerializedDictionary<Items, ItemInfo> itemInfo;

    void Start()
    {

    }

    void Update()
    {

    }

    public void spawnItem(Vector2 pos, Items item, int? ammo = null)
    {
        GameObject i = Instantiate(itemPrefab, grid.GetCellCenterWorld(
            grid.WorldToCell(new Vector3(pos.x, pos.y, 0))), Quaternion.identity);
        ItemInstance instance = i.GetComponent<ItemInstance>();
        instance.info = itemInfo[item];
        instance.ammo = ammo;
    }

    public void spawnBullet(Vector2 bulletSpawnPos, Vector2 bulletDirection, string shooterTag) {
        GameObject bulletObj = Instantiate(bulletPrefab, new Vector3(bulletSpawnPos.x, bulletSpawnPos.y, 0),
            Quaternion.identity);
        bulletObj.transform.up = bulletDirection;
        Bullet bullet = bulletObj.GetComponent<Bullet>(); 
        bullet.shooterTag = shooterTag;
        bullet.startBullet();
    }

    public Sprite getGroundItemSprite(Items item) {
        return itemInfo[item].groundSprite;
    }
}
