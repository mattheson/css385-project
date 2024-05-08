using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableWallTilemap : MonoBehaviour
{
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
        if (!destructibleTilemap.HasTile(tilePosition)) return;

        if (!health.ContainsKey(tilePosition)) {
            health[tilePosition] = tileStartHealth;
        }

        health[tilePosition] -= 1f;

        if (health[tilePosition] <= 0)
        {
            destructibleTilemap.SetTile(tilePosition, null);
            health.Remove(tilePosition);
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
