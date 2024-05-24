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
using UnityEngine.Animations;
using NUnit.Framework.Constraints;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

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
        guardPrefab, prisonerPrefab, playerPrefab, pistolPrefab, shotgunPrefab;
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
    [SerializeField] AudioClip soundtrack;
    [SerializeField] AudioMixerGroup soundtrackAudioGroup, soundEffectAudioGroup;
    AudioSource sound;

    // queue of available cells we can assign to prisoners
    Queue<Bounds> availableCells = new Queue<Bounds>();
    Queue<GuardPath> availablePaths = new Queue<GuardPath>();

    private TimeSpan time;
    private Game.Phase _phase;
    public Game.Phase phase { get => _phase; }

    private DateTime startTime;
    public TimeSpan elapsedTime;
    public bool isTimerRunning = false;

    private bool _playerFailedToCollectGold;

    // SerializedDictionary was downloaded from the asset store, it has an MIT license: 
    // https://github.com/ayellowpaper/SerializedDictionary
    [SerializeField]
    [SerializedDictionary("Item", "Item Info")]
    SerializedDictionary<Game.Items, ItemInfo> itemInfo;

    [SerializeField] Tilemap freeTimeZone, mineZone, officeZone;
    private List<Vector3> freeTimeTiles, mineTiles, officeTiles;
    private Bounds officeBounds;

    private const float minutesInDay = 24 * 60;

    // called when the phase changes
    public UnityEvent phaseChanged = new UnityEvent();

    void addMinute()
    {
        Game.Phase phaseBefore = _phase;
        time += TimeSpan.FromMinutes(1);
        // 8 am - 3:59 pm: Work
        // 4 pm - 6:59 pm: FreeTime
        // 7 pm - 7:59 pm: Return to Cell
        // 8 pm - 8 am: Nighttime
        if (time.Hours >= 8 && time.Hours < 16)
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
            if (hud && hud.quotaMetText.enabled)
            {
                hud.quotaMetText.enabled = false;
            }
            _phase = Game.Phase.ReturnToCell;
        }
        else
        {
            _phase = Game.Phase.Nighttime;
        }
        if (_phase != phaseBefore) {
            phaseChanged.Invoke();
        }
    }

    void Start()
    {
        _playerFailedToCollectGold = false;

        Time.timeScale = 1f;
        // generate lists of tiles 
        freeTimeTiles = generateListOfTilePositions(freeTimeZone);
        mineTiles = generateListOfTilePositions(mineZone);
        officeTiles = generateListOfTilePositions(officeZone);

        Vector3 offMin = officeZone.transform.TransformPoint(officeZone.localBounds.min);
        Vector3 offMax = officeZone.transform.TransformPoint(officeZone.localBounds.max);

        // officeBounds = new Bounds(
        //     new Vector3((offMin.x + offMax.x) / 2, (offMin.y + offMax.y) / 2, 0),
        //     new Vector3(offMax.x - offMin.x, offMax.y - offMin.y, 0)
        // );

        officeBounds = officeZone.gameObject.GetComponent<TilemapCollider2D>().bounds;
        Debug.Log(officeBounds);

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
            Guard newGuard = Instantiate(guardPrefab, guardSpawnPoint.position,
                Quaternion.identity).GetComponent<Guard>();
            newGuard.path = availablePaths.Dequeue();
        }

        // spawn 5 bounded guards
        for (int i = 0; i < 5; i++) {
            Guard newGuard = Instantiate(guardPrefab, guardSpawnPoint.position, Quaternion.identity).GetComponent<Guard>();
            newGuard.boundedToOffice = true;
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
                    new Vector3(cell.center.x, cell.center.y, 0), Quaternion.identity).GetComponent<Player>();
                player.cell = cell;
            }
            else
            {
                Prisoner newPrisoner = Instantiate(prisonerPrefab,
                    new Vector3(cell.center.x, cell.center.y, 0), Quaternion.identity).GetComponent<Prisoner>();
                newPrisoner.cell = cell;
            }
            cellIdx++;
        }

        time = new TimeSpan(startingHour, 0, 0);
        InvokeRepeating("addMinute", Game.minuteLengthInSeconds, Game.minuteLengthInSeconds);

        //Start the timer when the game starts
        StartTimer();

        sound = gameObject.AddComponent<AudioSource>();
        sound.outputAudioMixerGroup = soundtrackAudioGroup;
        sound.clip = soundtrack;
        sound.loop = true;
        sound.volume = 1f;
        sound.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadSceneAsync("Game");
        }

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

        // [ for work
        // ] for free time
        // \ for return to cell (after which comes night time)
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            time = new TimeSpan(8, 0, 0);
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

        if (isTimerRunning)
        {
            UpdateTimer();
        }
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
        else if (random > 0.25 && random <= 0.5)
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
    // public Bounds? getBoundsOfCurrentPhase()
    // {
    //     if (phaseZones.ContainsKey(phase))
    //     {
    //         return phaseZones[phase].cellBounds;
    //     }
    //     else
    //     {
    //         return null;
    //     }
    // }

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

    //Start the timer
    public void StartTimer()
    {
        startTime = DateTime.Now;
        isTimerRunning = true;
    }

    // update timer as the game goes
    public void UpdateTimer()
    {
        elapsedTime = DateTime.Now - startTime;
    }

    //method to stop timer when player die or escaped
    public void StopTimer()
    {
        isTimerRunning = false;
    }

    // returns center coordinate of random tile corresponding to phase
    // only for free time and work
    public Vector3 findRandomTileInPhase(Game.Phase phase)
    {
        Assert.IsTrue(phase == Game.Phase.FreeTime || phase == Game.Phase.Work);

        if (phase == Game.Phase.FreeTime) {
            return freeTimeTiles[UnityEngine.Random.Range(0, freeTimeTiles.Count)];
        }
        
        return mineTiles[UnityEngine.Random.Range(0, mineTiles.Count)];
    }

    public Vector3 findRandomTileInOffice() {
        return officeTiles[UnityEngine.Random.Range(0, officeTiles.Count)];
    }

    // for spawning pistol or shotgun
    public void guardDied(Vector3 lastPos) {
        float rand = UnityEngine.Random.value;
        if (rand < 0.25) {
            Instantiate(shotgunPrefab, lastPos, Quaternion.identity);
        } if (rand >= 0.25 && rand < 0.75) {
            Instantiate(pistolPrefab, lastPos, Quaternion.identity);
        }
    }

    public Bounds getOfficeBounds() {

        return officeBounds;
    }

    private List<Vector3> generateListOfTilePositions(Tilemap tilemap) {
        List<Vector3> l = new List<Vector3>();
        for (int x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; x++) {
            for (int y = tilemap.cellBounds.yMin; y < tilemap.cellBounds.yMax; y++) {
                Vector3Int t = new Vector3Int(x, y, (int) tilemap.transform.position.y);
                if (tilemap.HasTile(t)) {
                    l.Add(tilemap.GetCellCenterWorld(t));
                }
            }
        }
        return l;
    }

    public AudioMixerGroup getSoundEffectAudioGroup() {
        return soundEffectAudioGroup;
    }
}
