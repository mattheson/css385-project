using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Player : Character
{
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
    }
    public override void OnWalkedOverItem(GameObject item) {

    }
}
