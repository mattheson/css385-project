using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    private float wallHealth = 1f;

    public void pickaxeHit() {
        // subtract from wallHealth and change alpha here
        // destroy if 0 health
    }
}
