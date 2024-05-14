using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;
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
    [SerializeField] SpriteRenderer leftArmRenderer, rightArmRenderer, hairRenderer, handsRenderer;
    [SerializeField] public Sprite leftArmSprite, rightArmSprite, hairSprite, idleHandsSprite;

    private Animator animator;

    private bool punchingWithRight = true;

    private bool inAnimation
    {
        get
        {
            return
                !animator.GetCurrentAnimatorStateInfo(1).IsName("Idle") &&
                !animator.GetCurrentAnimatorStateInfo(1).IsName("Pistol Idle Right") &&
                !animator.GetCurrentAnimatorStateInfo(1).IsName("Pickaxe Idle") && 
                !animator.GetCurrentAnimatorStateInfo(1).IsName("Two Hand Stone Idle") &&
                !animator.GetCurrentAnimatorStateInfo(1).IsName("Master Key Idle") &&
                !animator.GetCurrentAnimatorStateInfo(1).IsName("Shotgun Idle")
            ;
        }
    }

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
                if (character.equippedItem == Game.Items.Pistol)
                {
                    rightArmRenderer.enabled = true;
                    animator.Play("Pistol Idle Right", 1);
                }
                if (character.equippedItem == Game.Items.Shotgun) {
                    rightArmRenderer.enabled = true;
                    leftArmRenderer.enabled = true;
                    animator.Play("Shotgun Idle");
                }
                if (character.equippedItem == Game.Items.Pickaxe)
                {
                    rightArmRenderer.enabled = true;
                    animator.Play("Pickaxe Idle", 1);
                }
                if (character.equippedItem == Game.Items.TwoHandStone)
                {
                    rightArmRenderer.enabled = true;
                    leftArmRenderer.enabled = true;
                    animator.Play("Two Hand Stone Idle", 1);
                }
                if (character.equippedItem == Game.Items.MasterKey)
                {
                    rightArmRenderer.enabled = true;
                    animator.Play("Master Key Idle", 1);
                }
            }
            else
            {
                rightArmRenderer.enabled = false;
                leftArmRenderer.enabled = false;
                animator.Play("Idle", 1);
            }
        }
    }

    public void clearAllAnimations()
    {
        leftArmRenderer.enabled = false;
        rightArmRenderer.enabled = false;
        animator.Play("Idle", 0);
        animator.Play("Idle", 1);
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
        if (!inAnimation)
        {
            rightArmRenderer.enabled = true;
            animator.Play("Punch Right", 1);
        }
    }

    private void punchLeft()
    {
        if (!inAnimation)
        {
            leftArmRenderer.enabled = true;
            animator.Play("Punch Left", 1);
        }
    }

    // toggles between left and right
    public void punch()
    {
        if (!inAnimation)
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

    public void shootPistol()
    {
        if (!inAnimation)
        {
            rightArmRenderer.enabled = true;
            animator.Play("Pistol Shoot Right", 1);
        }
    }

    public void shootShotgun() {
        if (!inAnimation) {
            rightArmRenderer.enabled = true;
            leftArmRenderer.enabled = true;
            animator.Play("Shotgun Shoot", 1);
        }
    }

    public void swingPickaxe()
    {
        if (!inAnimation)
        {
            rightArmRenderer.enabled = true;
            animator.Play("Pickaxe Swing", 1);
        }
    }

    public void swingTwoHandStone()
    {
        if (!inAnimation)
        {
            rightArmRenderer.enabled = true;
            leftArmRenderer.enabled = true;
            animator.Play("Two Hand Stone Swing", 1);
        }
    }

    public void reloadPistol()
    {
        if (!inAnimation)
        {
            leftArmRenderer.enabled = true;
            rightArmRenderer.enabled = true;
            animator.Play("Pistol Reload Right", 1);
        }
    }

    public void loadShotgun() {
        if (!inAnimation) {
            leftArmRenderer.enabled = true;
            rightArmRenderer.enabled = true;
            animator.Play("Shotgun Load", 1);
        }
    }

    public bool isSwingingTwoHandStone()
    {
        return animator.GetCurrentAnimatorStateInfo(1).IsName("Two Hand Stone Swing");
    }

    // Animation Events

    public void rightArmAnimationDone()
    {
        rightArmRenderer.enabled = false;
        animator.Play("Idle", 1);
    }

    public void leftArmAnimationDone()
    {
        leftArmRenderer.enabled = false;
        animator.Play("Idle", 1);
    }

    public void bothHandAnimationDone()
    {
        leftArmRenderer.enabled = false;
        rightArmRenderer.enabled = false;
        animator.Play("Idle", 1);
    }

    public void animationRightPunchImpact()
    {
        character.rightPunchImpact();
    }

    public void animationLeftPunchImpact()
    {
        character.leftPunchImpact();
    }

    public void animationPistolShot()
    {
        character.spawnPistolBullet();
    }

    public void animationPistolReload() {
        character.pistolReload();
    }

    public void animationShotgunLoad() {
        character.shotgunLoad();
    }
    
    public void animationShotgunShot() {
        character.spawnShotgunShot();
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
