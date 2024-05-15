using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System;
using UnityEngine.SceneManagement;

public class Player : Character
{
    public int gold = 0;

    // total ammo in reserve
    public Dictionary<Game.Items, int> reserve = new Dictionary<Game.Items, int>();

    // number of bullets in current clip/number of shells in chamber
    public Dictionary<Game.Items, int> clip = new Dictionary<Game.Items, int>();

    // inventory just contains equippable items as of now
    public List<Game.Items> inventory = new List<Game.Items>();

    private HUD hud;

    public Bounds cell;

    public override void OnStart()
    {
        reserve[Game.Items.Pistol] = 0;
        clip[Game.Items.Pistol] = 0;

        reserve[Game.Items.Shotgun] = 0;
        clip[Game.Items.Shotgun] = 0;

        hud = FindFirstObjectByType<HUD>();
        hud.player = this;
    }

    public override void OnUpdate()
    {
        health = 100;
        if (!hud) hud = FindAnyObjectByType<HUD>();

        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.LoadScene("Game");
        }

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

        // if (Input.GetKeyDown(KeyCode.M))
        // {
        //     FindFirstObjectByType<GameController>().spawnItem(
        //         new Vector2(transform.position.x, transform.position.y + 5),
        //         Game.Items.Pistol, 48);
        // }

        // if (Input.GetKeyDown(KeyCode.N))
        // {
        //     FindFirstObjectByType<GameController>().spawnItem(
        //         new Vector2(transform.position.x, transform.position.y + 5),
        //         Game.Items.Pickaxe);
        // }

        // if (Input.GetKeyDown(KeyCode.O))
        // {
        //     FindFirstObjectByType<GameController>().spawnItem(
        //         new Vector2(transform.position.x, transform.position.y + 5),
        //         Game.Items.Gold);
        // }

        // if (Input.GetKeyDown(KeyCode.P))
        // {
        //     FindFirstObjectByType<GameController>().spawnItem(
        //         new Vector2(transform.position.x, transform.position.y + 5),
        //         Game.Items.TwoHandStone);
        // }

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

    public void pickUpAmmo(Game.Items type, int ammo)
    {
        if (type == Game.Items.Pistol)
        {
            reserve[Game.Items.Pistol] = Math.Clamp(reserve.GetValueOrDefault(Game.Items.Pistol, 0) + ammo, 0,
                Game.maxPistolClips * Game.pistolClipSize);
        }
        else if (type == Game.Items.Shotgun)
        {
            reserve[Game.Items.Shotgun] = Math.Clamp(reserve.GetValueOrDefault(Game.Items.Shotgun, 0) + ammo, 0, Game.maxShotgunShells);
        }
        else
        {
            Debug.LogError("tried to add ammo of type " + type.ToString());
        }
    }

    public override void OnWalkedOverItem(GameObject item)
    {
        ItemInstance instance = item.GetComponent<ItemInstance>();
        Game.Items itemType = instance.info.item;

        if (itemType == Game.Items.Gold)
        {
            gold++;
            Destroy(item);
        }

        if (itemType == Game.Items.Pistol)
        {
            if (!inventory.Contains(Game.Items.Pistol))
            {
                inventory.Add(Game.Items.Pistol);
            }
            pickUpAmmo(Game.Items.Pistol, (int)instance.ammo);
            Destroy(item);
        }
        if (itemType == Game.Items.Shotgun)
        {
            if (!inventory.Contains(Game.Items.Shotgun))
            {
                inventory.Add(Game.Items.Shotgun);
            }
            pickUpAmmo(Game.Items.Shotgun, (int)instance.ammo);
            Destroy(item);
        }

        if (itemType == Game.Items.Pickaxe)
        {
            if (!inventory.Contains(Game.Items.Pickaxe))
            {
                inventory.Add(Game.Items.Pickaxe);
                Destroy(item);
            }
        }

        if (itemType == Game.Items.TwoHandStone)
        {
            if (!inventory.Contains(Game.Items.TwoHandStone))
            {
                inventory.Add(Game.Items.TwoHandStone);
                Destroy(item);
            }
        }
        if (itemType == Game.Items.MasterKey) {
            if (inventory.Contains(Game.Items.MasterKey)) {
                Debug.LogError("two master keys?");
            }
            inventory.Add(Game.Items.MasterKey);
            Destroy(item);
        }
    }

    public override void OnDeath()
    {
    }

    public override void OnTriggerEnterExtra(Collider2D col)
    {
        if (col.gameObject.name.Equals("Win")) {
            hud.winText.enabled = true;
            Time.timeScale = 0f;
        }
    }
}
