using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Player : MonoBehaviour
{
    [SerializeField] Sprite rightArmSprite;
    [SerializeField] Sprite leftArmSprite;
    private Character character;
    private CharacterAnimator anim;
    void Start()
    {
        character = GetComponent<Character>();
        anim = transform.Find("Character Animations").GetComponent<CharacterAnimator>();
        anim.leftArmSprite = leftArmSprite;
        anim.rightArmSprite = rightArmSprite;
    }

    void Update()
    {
        character.move(Input.GetKey(KeyCode.W), Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.D), Input.GetKey(KeyCode.LeftShift));

        if (Input.GetKey(KeyCode.Space)) {
            anim.punchRight();
        }
    }
}
