using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    // used for controlling sprite animations
    // we are avoiding using the animator controller, managing animations with code should be simpler
    // note that all arm animations are manually handled with arm enable/disable 
    // make sure you set rightArmSprite and leftArmSprite

    [NonSerialized] public Sprite rightArmSprite;
    [NonSerialized] public Sprite leftArmSprite;
    [NonSerialized] public Sprite hairSprite;

    private Animator animator;
    private SpriteRenderer rightArm;
    private SpriteRenderer leftArm;
    private SpriteRenderer hair;

    private bool punchingWithRight = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        rightArm = transform.Find("Right Arm").GetComponent<SpriteRenderer>();
        leftArm = transform.Find("Left Arm").GetComponent<SpriteRenderer>();
        hair = transform.Find("Hair").GetComponent<SpriteRenderer>();

        rightArm.enabled = false;
        leftArm.enabled = false;
    }

    void Update()
    {
        rightArm.sprite = rightArmSprite;
        leftArm.sprite = leftArmSprite;
        if (hair) hair.sprite = hairSprite;
    }

    public void showLeftArm()
    {
        leftArm.enabled = true;
    }

    public void hideLeftArm()
    {
        leftArm.enabled = false;
    }

    public void showRightArm()
    {
        rightArm.enabled = true;
    }

    public void hideRightArm()
    {
        rightArm.enabled = false;
    }

    public void startWalking()
    {
        animator.Play("Walking", 0);
    }

    public void stopWalking()
    {
        animator.Play("Idle", 0);
    }

    private void punchRight()
    {
        showRightArm();
        animator.Play("Punch Right", 1);
        animator.SetLayerWeight(1, 1);
    }

    private void punchLeft()
    {
        showLeftArm();
        animator.Play("Punch Left", 1);
        animator.SetLayerWeight(1, 1);
    }

    // toggles between left and right
    public void punch()
    {
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Idle"))
        {
            if (punchingWithRight)
            {
                punchRight();
                punchingWithRight = false;
            }
            else
            {
                punchLeft();
                punchingWithRight = true;
            }
        }
    }

    public void resetPunch()
    {
        punchingWithRight = true;
    }

    // Animation Events:

    public void punchRightDone()
    {
        hideRightArm();
        animator.Play("Idle", 1);
        animator.SetLayerWeight(1, 0);
    }

    public void punchLeftDone()
    {
        hideLeftArm();
        animator.Play("Idle", 1);
        animator.SetLayerWeight(1, 0);
    }

    // called when arm is fully extended in punch animation
    public void punchImpact() {

    }
}
