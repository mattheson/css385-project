using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using NavMeshPlus.Extensions;
using UnityEngine.TextCore;

public class GameController : MonoBehaviour
{
    [SerializeField] Grid grid;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Material deadCharacterMaterial;

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
    public Phase phase = Phase.Mealtime;

    void Start()
    {
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            phase = Phase.Mealtime;
        }
        else if(Input.GetKeyDown(KeyCode.Keypad1))
        {
            phase = Phase.Work;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            phase = Phase.Nighttime;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            phase = Phase.FreeTime;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            phase = Phase.ReturnToCell;
        }
    }

    public void spawnItem(Vector2 pos, Items item, int ammo = 0)
    {
        GameObject i = Instantiate(itemPrefab, grid.GetCellCenterWorld(
            grid.WorldToCell(new Vector3(pos.x, pos.y, 0))), Quaternion.identity);
        ItemInstance instance = i.GetComponent<ItemInstance>();
        instance.info = itemInfo[item];
        instance.ammo = ammo;
    }

    public void spawnBullet(Vector2 bulletSpawnPos, Vector2 bulletDirection, Character firer)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, new Vector3(bulletSpawnPos.x, bulletSpawnPos.y, 0),
            Quaternion.identity);
        bulletObj.transform.up = bulletDirection;
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.firer = firer;
        bullet.startBullet();
    }

    public Sprite getGroundItemSprite(Items item)
    {
        return itemInfo[item].groundSprite;
    }


    public void playerSwungPickaxe(Vector2 playerPos, Vector2 direction)
    {
        // find the closest tile that the player is facing
        // make sure they're within meleeDistance of the wall
        // find the tile and fade it
        // Find the player object
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Get the PolygonCollider2D component from the player object
            PolygonCollider2D playerCollider = player.GetComponent<PolygonCollider2D>();
            if (playerCollider != null)
            {
                Vector2 pickaxeDirection = direction.normalized;
                // Calculate the maximum distance from the center to any vertex
                float maxDistance = 0f;
                foreach (Vector2 vertex in playerCollider.points)
                {
                    float distance = Vector2.Distance(playerCollider.transform.TransformPoint(vertex), playerCollider.transform.position);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                    }
                }

                float raycastOffset = maxDistance;
                Vector2 raycastOrigin = playerPos + pickaxeDirection * raycastOffset;

                Debug.Log(LayerMask.GetMask("BreakableWallTilemap"));
                RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, pickaxeDirection, Game.meleeDistance, LayerMask.GetMask("BreakableWallTilemap"));
                if (hit.collider != null)
                {
                    Debug.Log("here");
                    BreakableWallTilemap tilemap = hit.collider.GetComponent<BreakableWallTilemap>();
                    if (tilemap != null)
                    {
                        tilemap.pickaxeHit(grid.WorldToCell(hit.point));
                    }
                }
            }
        }
    }

    public Material getDeadCharacterMaterial()
    {
        return deadCharacterMaterial;
    }

    public Vector3 getNextPatrolPosition(Guard guard)
    {
        // TODO
        // allocate this guard to a patrol route
        // store some index and return it
        // increment the index when the guard reaches the position
        return guard.transform.position;
    }

    public void freePatroller(Guard guard)
    {
        // TODO
        // called when guard dies
    }
    public Phase getCurrentPhase()
    {
        return phase;
    }
}
