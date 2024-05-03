using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    // used for setting up Sprite references and controlling some animations
    // make sure you set rightArmSprite and leftArmSprite

    public Sprite rightArmSprite;
    public Sprite leftArmSprite;
    public Sprite hairSprite;

    private Animator animator;
    private SpriteRenderer rightArm;
    private SpriteRenderer leftArm;
    private SpriteRenderer hair;

    void Start()
    {
        animator = GetComponent<Animator>();
        rightArm = transform.Find("Right Arm").GetComponent<SpriteRenderer>();
        leftArm = transform.Find("Left Arm").GetComponent<SpriteRenderer>();
        hair = transform.Find("Hair").GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        rightArm.sprite = rightArmSprite; 
        leftArm.sprite = leftArmSprite; 
        if (hair) hair.sprite = hairSprite;
    }

    public void showLeftArm() {
        leftArm.enabled = true;
    }

    public void hideLeftArm() {
        leftArm.enabled = false;
    }

    public void showRightArm() {
        rightArm.enabled = true;
    }

    public void hideRightArm() {
        rightArm.enabled = false;
    }

    public void startWalking() {
        animator.Play("Walking", 0);
    }

    public void stopWalking() {
        animator.Play("Idle", 0);
    }

    public void punchRight() {
        animator.Play("Punch Right", 1);
    }

    public void idleHands() {
        animator.Play("Idle", 1);
        Debug.Log("hi");
    }
}
