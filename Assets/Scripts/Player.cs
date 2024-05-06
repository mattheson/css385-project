using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System;

public class Player : Character
{
    public int gold = 0;

    // total ammo in reserve
    public Dictionary<Items, int> reserve = new Dictionary<Items, int>();

    // number of bullets in current clip/number of shells in chamber
    public Dictionary<Items, int> clip = new Dictionary<Items, int>();

    // inventory just contains equippable items as of now
    public List<Items> inventory = new List<Items>();

    private HUD hud;

    void Start()
    {
        reserve[Items.Pistol] = 0;
        clip[Items.Pistol] = 0;

        reserve[Items.Shotgun] = 0;
        clip[Items.Shotgun] = 0;
    }

    public override void OnUpdate()
    {
        if (!hud) hud = FindAnyObjectByType<HUD>();

        move(Input.GetKey(KeyCode.W), Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D), Input.GetKey(KeyCode.LeftShift));

        if (Input.GetKey(KeyCode.Space))
        {
            useItem();
        }
        else
        {
            idleItem();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            FindFirstObjectByType<GameController>().spawnItem(
                new Vector2(transform.position.x, transform.position.y + 5),
                Items.Pistol, 48);
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            FindFirstObjectByType<GameController>().spawnItem(
                new Vector2(transform.position.x, transform.position.y + 5),
                Items.Pickaxe);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            FindFirstObjectByType<GameController>().spawnItem(
                new Vector2(transform.position.x, transform.position.y + 5),
                Items.Gold);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            FindFirstObjectByType<GameController>().spawnItem(
                new Vector2(transform.position.x, transform.position.y + 5),
                Items.TwoHandStone);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            reloadItem();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && inventory.Count >= 1)
        {
            if (hud.highlighted == 0)
            {
                hud.highlighted = null;
                equippedItem = null;
            }
            else
            {
                hud.highlighted = 0;
                equippedItem = inventory[0];
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && inventory.Count >= 2)
        {
            if (hud.highlighted == 1)
            {
                hud.highlighted = null;
                equippedItem = null;
            }
            else
            {
                hud.highlighted = 1;
                equippedItem = inventory[1];
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && inventory.Count >= 3)
        {
            if (hud.highlighted == 2)
            {
                hud.highlighted = null;
                equippedItem = null;
            }
            else
            {
                hud.highlighted = 2;
                equippedItem = inventory[2];
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha4) && inventory.Count >= 4)
        {
            if (hud.highlighted == 3)
            {
                hud.highlighted = null;
                equippedItem = null;
            }
            else
            {
                hud.highlighted = 3;
                equippedItem = inventory[3];
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha5) && inventory.Count >= 5)
        {
            if (hud.highlighted == 4)
            {
                hud.highlighted = null;
                equippedItem = null;
            }
            else
            {
                hud.highlighted = 4;
                equippedItem = inventory[4];
            }
        }
    }

    public void pickUpAmmo(Items type, int ammo)
    {
        if (type == Items.Pistol)
        {
            reserve[Items.Pistol] = Math.Clamp(reserve.GetValueOrDefault(Items.Pistol, 0) + ammo, 0,
                Game.maxPistolClips * Game.pistolClipSize);
        }
        else if (type == Items.Shotgun)
        {
            reserve[Items.Shotgun] = Math.Clamp(reserve.GetValueOrDefault(Items.Shotgun, 0) + ammo, 0, Game.maxShotgunShells);
        }
        else
        {
            Debug.LogError("tried to add ammo of type " + type.ToString());
        }
    }

    public override void OnWalkedOverItem(GameObject item)
    {
        ItemInstance instance = item.GetComponent<ItemInstance>();
        Items itemType = instance.info.item;

        if (itemType == Items.Gold)
        {
            gold++;
            Destroy(item);
        }

        if (itemType == Items.Pistol)
        {
            if (!inventory.Contains(Items.Pistol))
            {
                inventory.Add(Items.Pistol);
            }
            pickUpAmmo(Items.Pistol, (int)instance.ammo);
            Destroy(item);
        }

        if (itemType == Items.Pickaxe)
        {
            if (!inventory.Contains(Items.Pickaxe))
            {
                inventory.Add(Items.Pickaxe);
                Destroy(item);
            }
        }

        if (itemType == Items.TwoHandStone)
        {
            if (!inventory.Contains(Items.TwoHandStone))
            {
                inventory.Add(Items.TwoHandStone);
                Destroy(item);
            }
        }
    }
}
