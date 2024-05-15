using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    private int _numSlots = 0;
    public int numSlots
    {
        get => _numSlots;
        set
        {
            _numSlots = value;
            while (slots.Count < _numSlots) addSlot();
            while (slots.Count > _numSlots) deleteLastSlot();
        }
    }
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Transform slotsStart;
    [SerializeField] float slotPaddingPercent;
    [SerializeField] public Player player;
    [SerializeField] GameController controller;
    [SerializeField] public TMP_Text goldText, pistolAmmoText, shotgunAmmoText, healthText, winText, timeText, phaseText;

    public int? highlighted;

    private List<HUDSlot> slots = new List<HUDSlot>();

    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (!GetComponent<Canvas>().worldCamera) {
            GameObject camera = GameObject.FindWithTag("MainCamera");
            if (!camera) {
                Debug.LogError("HUD: could not find main camera");
            } else {
                canvas.worldCamera = camera.GetComponent<Camera>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }
        }
        numSlots = 5;
    }

    void Update() {
        if (!controller) controller = FindFirstObjectByType<GameController>();
        if (!player) return;

        goldText.text = player.gold.ToString();

        int pistolClip = player.clip.GetValueOrDefault(Game.Items.Pistol, 0);
        int pistolReserve = player.reserve.GetValueOrDefault(Game.Items.Pistol, 0);
        int shotgunChambered = player.clip.GetValueOrDefault(Game.Items.Shotgun, 0);
        int shotgunReserve = player.reserve.GetValueOrDefault(Game.Items.Shotgun, 0);

        pistolAmmoText.text = pistolClip + " / " + pistolReserve;
        shotgunAmmoText.text = shotgunChambered + " / " + shotgunReserve;
        healthText.text = player.health.ToString();

        int i = 0;

        for (; i < numSlots && i < player.inventory.Count; i++) {
            slots[i].itemImage.sprite = controller.getGroundItemSprite(player.inventory[i]);
            slots[i].itemImage.enabled = true;
            if (highlighted == i) {
                slots[i].highlightImage.enabled = true;
            } else {
                slots[i].highlightImage.enabled = false;
            }
        }

        while (i < numSlots) {
            slots[i++].itemImage.enabled = false;
        }

        timeText.SetText(controller.getTime().ToString(@"hh\:mm"));
        phaseText.SetText(Game.PhaseNames[(int) controller.phase]);
    }

    private void addSlot()
    {
        GameObject slot = Instantiate(slotPrefab, transform);
        RectTransform slotTr = slot.GetComponent<RectTransform>();
        RectTransform thisTr = GetComponent<RectTransform>();
        slotTr.anchorMin = thisTr.anchorMin;
        slotTr.anchorMax = thisTr.anchorMax;
        slotTr.pivot = thisTr.pivot;
        if (slots.Count == 0)
        {
            slotTr.anchoredPosition = slotsStart.GetComponent<RectTransform>().anchoredPosition;
        }
        else
        {
            Vector2 newPos = slots[slots.Count - 1].GetComponent<RectTransform>().anchoredPosition;
            float width = slotPrefab.GetComponent<RectTransform>().rect.width;
            newPos.x += width + (width * slotPaddingPercent);
            slotTr.anchoredPosition = newPos;
        }

        HUDSlot hudslot = slot.GetComponent<HUDSlot>();
        slots.Add(hudslot);

        hudslot.itemImage.enabled = false;
        hudslot.slotNumberText.text = slots.Count.ToString();
    }

    private void deleteLastSlot() {
        Destroy(slots[slots.Count - 1].gameObject);
        slots.RemoveAt(slots.Count - 1);
    }
}