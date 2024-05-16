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

    // if we got call to reset punch during an animation
    private bool queuedReset = false;

    private bool inAnimation
    {
        get
        {
            return
            !(
                animator.GetCurrentAnimatorStateInfo(1).IsName("Idle") ||
                animator.GetCurrentAnimatorStateInfo(1).IsName("Pistol Idle Right") ||
                animator.GetCurrentAnimatorStateInfo(1).IsName("Pickaxe Idle") ||
                animator.GetCurrentAnimatorStateInfo(1).IsName("Two Hand Stone Idle") ||
                animator.GetCurrentAnimatorStateInfo(1).IsName("Master Key Idle") ||
                animator.GetCurrentAnimatorStateInfo(1).IsName("Shotgun Idle")
            );
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
        Debug.Log(inAnimation);
        leftArmRenderer.sprite = leftArmSprite;
        rightArmRenderer.sprite = rightArmSprite;
        hairRenderer.sprite = hairSprite;

        if (!character) return;

        if (character.equippedItem == null)
        {
            if (!inAnimation)
            {
                rightArmRenderer.enabled = false;
                leftArmRenderer.enabled = false;
                animator.Play("Idle", 1);
            }
            else if (punchingWithRight)
            {
                rightArmRenderer.enabled = true;
                leftArmRenderer.enabled = false;
            }
            else if (!punchingWithRight)
            {
                rightArmRenderer.enabled = false;
                leftArmRenderer.enabled = true;
            }
        }
        else if (character.equippedItem == Game.Items.Pistol)
        {
            rightArmRenderer.enabled = true;
            leftArmRenderer.enabled = false;
            if (!inAnimation)
            {
                animator.Play("Pistol Idle Right", 1);
            }
        }
        else if (character.equippedItem == Game.Items.Shotgun)
        {
            rightArmRenderer.enabled = true;
            leftArmRenderer.enabled = true;
            if (!inAnimation)
            {
                animator.Play("Shotgun Idle", 1);
            }
        }
        else if (character.equippedItem == Game.Items.Pickaxe)
        {
            rightArmRenderer.enabled = true;
            leftArmRenderer.enabled = false;
            if (!inAnimation)
            {
                animator.Play("Pickaxe Idle", 1);
            }
        }
        else if (character.equippedItem == Game.Items.TwoHandStone)
        {
            rightArmRenderer.enabled = true;
            leftArmRenderer.enabled = true;
            if (!inAnimation)
            {
                animator.Play("Two Hand Stone Idle", 1);
            }
        }
        else if (character.equippedItem == Game.Items.MasterKey)
        {
            rightArmRenderer.enabled = true;
            leftArmRenderer.enabled = false;
            if (!inAnimation)
            {
                animator.Play("Master Key Idle", 1);
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
        if (animator) animator.Play("Idle", 0);
    }

    private void punchRight()
    {
        if (!inAnimation)
        {
            animator.Play("Punch Right", 1);
        }
    }

    private bool isPunchingRight()
    {
        return animator.GetCurrentAnimatorStateInfo(1).IsName("Punch Right");
    }

    private void punchLeft()
    {
        if (!inAnimation)
        {
            animator.Play("Punch Left", 1);
        }
    }

    private bool isPunchingLeft()
    {
        return animator.GetCurrentAnimatorStateInfo(1).IsName("Punch Left");
    }

    // toggles between left and right
    public void punch()
    {
        if (!inAnimation)
        {
            if (punchingWithRight)
            {
                punchRight();
            }
            else
            {
                punchLeft();
            }
        }
    }

    public void resetPunch()
    {
        if (inAnimation)
        {
            queuedReset = true;
        } else {
            punchingWithRight = true;
        }
    }

    public void shootPistol()
    {
        if (!inAnimation)
        {
            animator.Play("Pistol Shoot Right", 1);
        }
    }

    private bool isShootingPistol()
    {
        return animator.GetCurrentAnimatorStateInfo(1).IsName("Pistol Shoot Right");
    }

    public void shootShotgun()
    {
        if (!inAnimation)
        {
            animator.Play("Shotgun Shoot", 1);
        }
    }

    private bool isShootingShotgun()
    {
        return animator.GetCurrentAnimatorStateInfo(1).IsName("Shotgun Shoot");
    }

    public void swingPickaxe()
    {
        if (!inAnimation)
        {
            animator.Play("Pickaxe Swing", 1);
        }
    }
    private bool isSwingingPickaxe()
    {
        return animator.GetCurrentAnimatorStateInfo(1).IsName("Pickaxe Swing");
    }

    public void swingTwoHandStone()
    {
        if (!inAnimation)
        {
            animator.Play("Two Hand Stone Swing", 1);
        }
    }

    public bool isSwingingTwoHandStone()
    {
        if (!animator) return false;
        return animator.GetCurrentAnimatorStateInfo(1).IsName("Two Hand Stone Swing");
    }

    public void reloadPistol()
    {
        if (!inAnimation)
        {
            animator.Play("Pistol Reload Right", 1);
        }
    }

    public void loadShotgun()
    {
        if (!inAnimation)
        {
            animator.Play("Shotgun Load", 1);
        }
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

    public void animationPistolReload()
    {
        character.pistolReload();
    }

    public void animationShotgunLoad()
    {
        character.shotgunLoad();
    }

    public void animationShotgunShot()
    {
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

    public void togglePunchHand()
    {
        if (queuedReset)
        {
            queuedReset = false;
        }
        else
        {
            punchingWithRight = !punchingWithRight;
        }
    }

    public void leftPunchDone()
    {
        punchingWithRight = true;
    }

    public void rightPunchDone()
    {
        punchingWithRight = false;
    }
}
