using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    private int _numSlots;
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
    public TMP_Text goldText;

    private List<HUDSlot> slots = new List<HUDSlot>();

    void Start()
    {
        numSlots = 5;
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
    }

    public void setGoldText(int num) {
        goldText.text = "Gold: " + num;
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