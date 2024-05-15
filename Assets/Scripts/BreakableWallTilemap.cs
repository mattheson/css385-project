using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableWallTilemap : MonoBehaviour
{
    private GameController controller;
    public Tilemap destructibleTilemap;
    private Dictionary<Vector3Int, float> health = new Dictionary<Vector3Int, float>();
    public float tileStartHealth;

    private void Start()
    {
        destructibleTilemap = GetComponent<Tilemap>();
    }

    // Method to handle pickaxe hit
    public void pickaxeHit(Vector3Int tilePosition)
    {
        if (!controller) controller = FindFirstObjectByType<GameController>();

        if (!destructibleTilemap.HasTile(tilePosition)) return;
        Debug.Log("hi there");

        if (!health.ContainsKey(tilePosition))
        {
            health[tilePosition] = tileStartHealth;
        }

        health[tilePosition] -= 1f;

        if (health[tilePosition] <= 0)
        {
            GameObject tile = destructibleTilemap.GetTile<Tile>(tilePosition).gameObject;

            destructibleTilemap.SetTile(tilePosition, null);
            health.Remove(tilePosition);

            // logic for stone tiles
            if (tile != null)
            {
                bool isStone = tile.CompareTag("Stone Tile");
                Debug.Log(tile);
                if (isStone)
                {
                    controller.stoneTileDestroyed(destructibleTilemap.GetCellCenterWorld(tilePosition));
                }
            }
        }
        else
        {
            Color originalColor = destructibleTilemap.GetColor(tilePosition);
            Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, health[tilePosition] / tileStartHealth);
            destructibleTilemap.SetColor(tilePosition, newColor);
            destructibleTilemap.RefreshTile(tilePosition);
        }
    }
}
