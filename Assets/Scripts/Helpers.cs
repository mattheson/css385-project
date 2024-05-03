using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class GameHelpers : MonoBehaviour
{
    private static float[] acceptableAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };
    private static bool4[] anglesToButtons = { 
        new bool4(false, false, false, true),
        new bool4(true, false, false, true),
        new bool4(true, false, false, false),
        new bool4(true, false, true, false),
        new bool4(false, false, true, false),
        new bool4(false, true, true, false),
        new bool4(false, true, false, false),
        new bool4(false, true, false, true)
    };

    // returns up, down, left, right
    public static bool4 convertDirectionToButtons(Vector2 direction) {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = (angle + 360) % 360;

        int minIdx = 0;
        float minDiff = Mathf.Abs(angle - acceptableAngles[0]);

        for (int i = 1; i < acceptableAngles.Length; i++) {
            float diff = Mathf.Abs(angle - acceptableAngles[i]);
            if (diff < minDiff) {
                minDiff = diff;
                minIdx = i;
            }
        }

        return anglesToButtons[minIdx];
    }
}
