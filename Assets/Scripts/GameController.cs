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
using System.Linq;

// contains logic for
// - associating item enum with ItemInfo (see comment below)
// - world time, getting current phase/time
// - spawning items and bullets
public class GameController : MonoBehaviour
{
    [Serializable]
    public class GuardPath
    {
        public List<Transform> points;
    }
    [SerializeField] Grid grid;
    [SerializeField] GameObject itemPrefab;
    [SerializeField]
    GameObject pistolBulletPrefab, shotgunBulletPrefab, goldPrefab, twoHandStonePrefab,
        guardPrefab, prisonerPrefab, playerPrefab;
    [SerializeField] Material deadCharacterMaterial;
    [SerializeField] Light2D worldLight;
    [SerializeField] Gradient worldLightGradient;
    [SerializeField] int startingHour;
    [SerializeField] List<Door> cellDoors;
    [SerializeField] List<GuardPath> guardPaths;

    // we are using this as a way to specify cell bounds
    // composite collider creates multiple physics shapes from one tilemap collider
    [SerializeField] CompositeCollider2D cellBounds;
    [SerializeField] Transform guardSpawnPoint;
    [SerializeField] Player player;

    // queue of available cells we can assign to prisoners
    Queue<Bounds> availableCells = new Queue<Bounds>();
    Queue<GuardPath> availablePaths = new Queue<GuardPath>();

    private TimeSpan time;
    private Game.Phase _phase;
    public Game.Phase phase { get => _phase; }

    private bool _playerFailedToCollectGold;

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

    [SerializeField]
    [SerializedDictionary("Phase", "Phase Zone")]
    SerializedDictionary<Game.Phase, TilemapCollider2D> phaseZones;

    private const float minutesInDay = 24 * 60;

    void addMinute()
    {
        time += TimeSpan.FromMinutes(1);
        // 8 am -  10:59am: Mealtime (breakfast)
        // 11 am - 3:59 pm: Work
        // 4 pm - 6:59 pm: FreeTime
        // 7 pm - 7:59 pm: Return to Cell
        // 8 pm - 8 am: Nighttime
        if (time.Hours >= 8 && time.Hours < 11)
        {
            _phase = Game.Phase.Mealtime;
        }
        else if (time.Hours >= 11 && time.Hours < 16)
        {
            HUD hud = FindFirstObjectByType<HUD>();
            if (hud)
            {
                if (!hud.quotaFailText.enabled)
                {
                    hud.quotaMetText.enabled = false;
                }
            }
            _phase = Game.Phase.Work;
        }
        else if (time.Hours >= 16 && time.Hours < 19)
        {
            HUD hud = FindFirstObjectByType<HUD>();
            if (!player) player = FindFirstObjectByType<Player>();
            if (player.gold < 3 && !hud.quotaMetText.enabled)
            {
                Debug.Log("failed to collect gold");
                _playerFailedToCollectGold = true;
                if (hud)
                {
                    hud.quotaFailText.enabled = true;
                }
            }
            else
            {
                if (hud)
                {
                    hud.quotaMetText.enabled = true;
                }
            }
            player.gold = 0;
            _phase = Game.Phase.FreeTime;

        }
        else if (time.Hours == 19)
        {
            HUD hud = FindFirstObjectByType<HUD>();
            if (hud && hud.quotaMetText.enabled) {
                hud.quotaMetText.enabled = false;
            }
            _phase = Game.Phase.ReturnToCell;
        }
        else
        {
            _phase = Game.Phase.Nighttime;
        }
    }

    void Start()
    {
        _playerFailedToCollectGold = false;

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
            availableCells.Enqueue(bounds);
        }

        foreach (GuardPath path in guardPaths)
        {
            Debug.Log("hi");
            availablePaths.Enqueue(path);
        }

        Debug.Log(availablePaths.Count);

        // spawn guards, allocate one per path
        while (availablePaths.Count > 0)
        {
            Debug.Log("spawning path");
            Guard newGuard = Instantiate(guardPrefab, guardSpawnPoint.position,
                Quaternion.identity).GetComponent<Guard>();
            newGuard.path = availablePaths.Dequeue();
        }

        // spawn prisoners, one per cell
        // also spawn player here

        int playerCell = UnityEngine.Random.Range(0, availableCells.Count - 1);
        int cellIdx = 0;
        while (availableCells.Count > 0)
        {
            Bounds cell = availableCells.Dequeue();
            if (cellIdx == playerCell)
            {
                Player player = Instantiate(playerPrefab,
                    cell.center, Quaternion.identity).GetComponent<Player>();
                player.cell = cell;
            }
            else
            {
                Prisoner newPrisoner = Instantiate(prisonerPrefab,
                    cell.center, Quaternion.identity).GetComponent<Prisoner>();
                newPrisoner.cell = cell;
            }
            cellIdx++;
        }

        time = new TimeSpan(startingHour, 0, 0);
        InvokeRepeating("addMinute", Game.minuteLengthInSeconds, Game.minuteLengthInSeconds);
    }

    void Update()
    {
        if (phase == Game.Phase.Nighttime)
        {
            foreach (Door d in cellDoors)
            {
                d.open(false);
            }
        }
        else
        {
            foreach (Door d in cellDoors)
            {
                d.open(true);
            }
        }

        // p for breakfast
        // [ for work
        // ] for free time
        // \ for return to cell (after which comes night time)
        if (Input.GetKeyDown(KeyCode.P))
        {
            time = new TimeSpan(8, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            time = new TimeSpan(11, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            time = new TimeSpan(16, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            time = new TimeSpan(19, 0, 0);
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

    // dont think we need this, maybe just have fixed number of guards
    // we arent respawning guards
    // public void freePatroller(Guard guard)
    // {
    //     // TODO
    //     // called when guard dies
    // }

    public void stoneTileDestroyed(Vector2 pos)
    {
        float random = UnityEngine.Random.value;

        if (random >= 0 && random <= 0.25)
        {
            Instantiate(goldPrefab, pos, Quaternion.identity);
        }
        else if (random > 0.25 && random <= 0.25)
        {
            Instantiate(twoHandStonePrefab, pos, Quaternion.identity);
        }

        Debug.Log(pos);
    }

    public TimeSpan getTime()
    {
        return time;
    }

    // returns null if there is no set bounds for a given phase
    public Bounds? getBoundsOfCurrentPhase()
    {
        if (phaseZones.ContainsKey(phase))
        {
            return phaseZones[phase].bounds;
        }
        else
        {
            return null;
        }
    }

    // whether the player missed collecting gold for one work period
    public bool playerFailedToCollectGold()
    {
        return _playerFailedToCollectGold;
    }


    // called by player when sleeping
    // does nothing when not nighttime
    public void sleep()
    {
        // TODO add fade out effect and make this look pretty
        if (phase == Game.Phase.Nighttime)
        {
            time = new TimeSpan(8, 0, 0);
        }
    }
}
