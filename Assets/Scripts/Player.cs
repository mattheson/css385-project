using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Items = ItemInfo.Items;

public class Player : Character
{
    [SerializeField] HUD hud;
    private int gold = 0;
    private List<ItemInfo.Items> inv;
    private Ammo pistolAmmo = Ammo.Zero();
    private Ammo shotgunAmmo = Ammo.Zero();

    void Update()
    {
        move(Input.GetKey(KeyCode.W), Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D), Input.GetKey(KeyCode.LeftShift));

        if (Input.GetKey(KeyCode.Space)) {
            punch();
        } else {
            resetPunch();
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            FindFirstObjectByType<GameController>().spawnItem(new Vector2(transform.position.x, transform.position.y), ItemInfo.Items.Pistol);
        }

        hud.setGoldText(gold);
    }
    public override void OnWalkedOverItem(GameObject item) {
        ItemInfo itemInfo = item.GetComponent<ItemInfo>();
        Items itemType = itemInfo.item;
        ItemInstance instance = item.GetComponent<ItemInstance>();

        if (itemType == Items.Gold) {
            gold++;
            Destroy(item);
        }

        if (itemType == Items.Pistol) {
            if (inv.Contains(Items.Pistol)) {
                pistolAmmo.reserve += instance.ammo.mag + instance.ammo.reserve;
            } else {
                inv.Add(Items.Pistol);
                pistolAmmo = instance.ammo;
            }
        }
    }
}
