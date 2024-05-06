using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

public class CharacterAnimator : MonoBehaviour
{
    // used for controlling sprite animations
    // we are avoiding using the animator controller, managing animations with code should be simpler
    // note that all arm animations are manually handled with arm enable/disable 

    // character receives events in animations
    [NonSerialized] public Character character;
    [NonSerialized] public string idleAnimationName;
    [SerializeField] SpriteRenderer leftArmRenderer, rightArmRenderer, hairRenderer, handsRenderer;
    [SerializeField] public Sprite leftArmSprite, rightArmSprite, hairSprite, idleHandsSprite;

    private Animator animator;

    private bool punchingWithRight = true;

    private bool inAnimation = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        rightArmRenderer.enabled = false;
        leftArmRenderer.enabled = false;
    }

    void Update()
    {
        leftArmRenderer.sprite = leftArmSprite;
        rightArmRenderer.sprite = rightArmSprite;
        hairRenderer.sprite = hairSprite;

        if (!character) return;
        if (!inAnimation)
        {
            leftArmRenderer.enabled = false;
            rightArmRenderer.enabled = false;
            if (character.equippedItem != null)
            {
                if (character.equippedItem == Items.Pistol)
                {
                    rightArmRenderer.enabled = true;
                    animator.Play("Pistol Idle Right", 1);
                }
                if (character.equippedItem == Items.Pickaxe)
                {
                    rightArmRenderer.enabled = true;
                    animator.Play("Pickaxe Idle", 1);
                }
                if (character.equippedItem == Items.TwoHandStone)
                {
                    rightArmRenderer.enabled = true;
                    leftArmRenderer.enabled = true;
                    animator.Play("Two Hand Stone Idle", 1);
                }
            }
            else
            {
                animator.Play("Idle", 1);
            }
        }
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
        rightArmRenderer.enabled = true;
        animator.Play("Punch Right", 1);
    }

    private void punchLeft()
    {
        leftArmRenderer.enabled = true;
        animator.Play("Punch Left", 1);
    }

    // toggles between left and right
    public void punch()
    {
        if (!inAnimation)
        {
            inAnimation = true;
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

    public void shootPistol()
    {
        if (!animator.GetCurrentAnimatorStateInfo(1).IsName("Pistol Shoot Right"))
        {
            inAnimation = true;
            rightArmRenderer.enabled = true;
            animator.Play("Pistol Shoot Right", 1);
        }
    }

    public void swingPickaxe()
    {
        if (!animator.GetCurrentAnimatorStateInfo(1).IsName("Pickaxe Swing"))
        {
            inAnimation = true;
            rightArmRenderer.enabled = true;
            animator.Play("Pickaxe Swing", 1);
        }
    }

    public void swingTwoHandStone()
    {
        if (!animator.GetCurrentAnimatorStateInfo(1).IsName("Two Hand Stone Swing"))
        {
            inAnimation = true;
            rightArmRenderer.enabled = true;
            leftArmRenderer.enabled = true;
            animator.Play("Two Hand Stone Swing", 1);
        }
    }


    public void reloadPistol()
    {
        if (!animator.GetCurrentAnimatorStateInfo(1).IsName("Pistol Reload Right"))
        {
            inAnimation = true;
            leftArmRenderer.enabled = true;
            rightArmRenderer.enabled = true;
            animator.Play("Pistol Reload Right", 1);
        }
    }

    // Animation Events

    public void rightArmAnimationDone()
    {
        rightArmRenderer.enabled = false;
        animator.Play("Idle", 1);
        inAnimation = false;
    }

    public void leftArmAnimationDone()
    {
        leftArmRenderer.enabled = false;
        animator.Play("Idle", 1);
        inAnimation = false;
    }

    public void bothHandAnimationDone()
    {
        leftArmRenderer.enabled = false;
        rightArmRenderer.enabled = false;
        animator.Play("Idle", 1);
        inAnimation = false;
    }

    public void animationPunchImpact()
    {
        character.punchImpact();
    }

    public void animationPistolShot()
    {
        character.spawnPistolBullet();
    }

    public void animationPickaxeImpact()
    {
        character.pickaxeImpact();
    }

    public void animationTwoHandStoneImpact()
    {
        character.twoHandStoneImpact();
    }
}
