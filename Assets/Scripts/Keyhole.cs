using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keyhole : MonoBehaviour
{
    public Door door; 
    public Items keyType;

    void OnTriggerEnter2D(Collider2D col) {
        Character maybeCharacter = col.GetComponent<Character>();
        if (maybeCharacter) {
            if (maybeCharacter.equippedItem == keyType) {
                door.toggleDoor();
            }
        }
    }
}
