using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine.Events;
using Unity.VisualScripting;

// contains logic for
// - associating item enum with ItemInfo (see comment below)
// - world time, getting current phase/time
// - spawning items and bullets
public class GameController : MonoBehaviour
{
    [SerializeField] Grid grid;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] GameObject pistolBulletPrefab, shotgunBulletPrefab, goldPrefab, twoHandStonePrefab;
    [SerializeField] Material deadCharacterMaterial;
    [SerializeField] Light2D worldLight;
    [SerializeField] Gradient worldLightGradient;
    [SerializeField] int startingHour;
    [SerializeField] List<Door> cellDoors;
    [SerializeField] List<List<Transform>> guardPaths;

    // we are using this as a way to specify cell bounds
    // composite collider creates multiple physics shapes from one tilemap collider
    [SerializeField] CompositeCollider2D cellBounds;

    // queue of available cells we can assign to prisoners
    Queue<Bounds> availableCells = new Queue<Bounds>();

    private TimeSpan time;
    private Game.Phase _phase;
    public Game.Phase phase { get => _phase; }

    // TODO this is the cleanest way of associating ItemInfo with an enum
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
    SerializedDictionary<Game.Items, ItemInfo> itemInfo;

    private const float minutesInDay = 24 * 60;

    void addMinute()
    {
        time += TimeSpan.FromMinutes(1);
        // 8 am -  10:59am: Mealtime
        // 11 am - 3:59 pm: Work
        // 4 pm - 7:59 pm: FreeTime
        // 8 pm - 8 am: Nighttime
        if (time.Hours >= 8 && time.Hours < 11)
        {
            _phase = Game.Phase.Mealtime;
        }
        else if (time.Hours >= 11 && time.Hours < 16)
        {
            _phase = Game.Phase.Work;
        }
        else if (time.Hours >= 16 && time.Hours < 21)
        {
            _phase = Game.Phase.FreeTime;
        }
        else
        {
            _phase = Game.Phase.Nighttime;
        }
    }

    void Start()
    {
        Time.timeScale = 1f;

        // populate available cells
        for (int i = 0; i < cellBounds.pathCount; i++)
        {
            float xMin = Mathf.Infinity, xMax = Mathf.NegativeInfinity,
                yMin = Mathf.Infinity, yMax = Mathf.NegativeInfinity;
            Vector2[] points = new Vector2[cellBounds.GetPathPointCount(i)];
            cellBounds.GetPath(i, points);
            foreach (Vector2 p in points)
            {
                xMin = Math.Min(xMin, p.x);
                xMax = Math.Max(xMax, p.x);
                yMin = Math.Min(yMin, p.y);
                yMax = Math.Max(yMax, p.y);
            }
            Bounds bounds = new Bounds(
                new Vector3((xMin + xMax) / 2, (yMin + yMax) / 2, 0),
                new Vector3((xMax - xMin) / 2, (yMax - yMin) / 2, 0)
            );
            // Debug.Log(bounds);
            availableCells.Enqueue(bounds);
        }

        time = new TimeSpan(startingHour, 0, 0);
        InvokeRepeating("addMinute", Game.minuteLengthInSeconds, Game.minuteLengthInSeconds);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket)) {
            foreach (Door d in cellDoors) {
                d.toggleDoor();
            }
        }
        worldLight.color = worldLightGradient.Evaluate((float)time.TotalMinutes % minutesInDay / minutesInDay);
    }

    public void spawnItem(Vector2 pos, Game.Items item, int ammo = 0)
    {
        GameObject i = Instantiate(itemPrefab, grid.GetCellCenterWorld(
            grid.WorldToCell(new Vector3(pos.x, pos.y, 0))), Quaternion.identity);
        ItemInstance instance = i.GetComponent<ItemInstance>();
        instance.info = itemInfo[item];
        instance.ammo = ammo;
    }

    public void spawnPistolBullet(Vector2 bulletSpawnPos, Vector2 bulletDirection, Character firer)
    {
        GameObject bulletObj = Instantiate(pistolBulletPrefab, new Vector3(bulletSpawnPos.x, bulletSpawnPos.y, 0),
            Quaternion.identity);
        bulletObj.transform.up = bulletDirection;
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.firer = firer;
        bullet.startBullet();
    }

    public void spawnShotgunShot(Vector2 shotSpawnPos, Vector2 shotDirection, Character firer)
    {
        for (int i = 0; i < Game.bulletsSpawnedShotgun; i++)
        {
            GameObject bulletObj = Instantiate(shotgunBulletPrefab, new Vector3(shotSpawnPos.x, shotSpawnPos.y, 0),
                Quaternion.identity);
            Debug.Log(bulletObj.transform.up);
            bulletObj.transform.up = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-8, 8)) * shotDirection;
            Debug.Log(bulletObj.transform.up);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.firer = firer;
            bullet.startBullet();
        }
    }

    public Sprite getGroundItemSprite(Game.Items item)
    {
        return itemInfo[item].groundSprite;
    }

    public Vector3Int worldToCell(Vector3 point)
    {
        return grid.WorldToCell(point);
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

    public void stoneTileDestroyed(Vector2 pos) {
        float random = UnityEngine.Random.value;

        if (random >= 0 && random <= 0.25) {
            Instantiate(goldPrefab, pos, Quaternion.identity);
        } else if (random > 0.25 && random <= 0.25) {
            Instantiate(twoHandStonePrefab, pos, Quaternion.identity);
        }

        Debug.Log(pos);
    }

    public TimeSpan getTime()
    {
        return time;
    }
}
