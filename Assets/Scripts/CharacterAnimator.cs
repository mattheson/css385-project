using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    // used for controlling sprite animations
    // we are avoiding using the animator controller, managing animations with code should be simpler
    // note that all arm animations are manually handled with arm enable/disable 

    // character receives events in animations
    [NonSerialized] public Character character;
    [SerializeField] SpriteRenderer leftArmRenderer, rightArmRenderer, hairRenderer;
    private Sprite _leftArmSprite, _rightArmSprite, _hairSprite;
    public Sprite rightArmSprite
    {
        get => _rightArmSprite; set
        {
            _rightArmSprite = value;
            rightArmRenderer.sprite = _rightArmSprite;
        }
    }
    public Sprite leftArmSprite
    {
        get => _leftArmSprite;
        set
        {
            _leftArmSprite = value;
            leftArmRenderer.sprite = _leftArmSprite;
        }
    }
    public Sprite hairSprite
    {
        get => _hairSprite;
        set
        {
            _hairSprite = value;
            hairRenderer.sprite = _hairSprite;
        }
    }

    private Animator animator;

    private bool punchingWithRight = true;

    void Start()
    {
        animator = GetComponent<Animator>();

        rightArmRenderer.enabled = false;
        leftArmRenderer.enabled = false;
    }

    public void showLeftArm()
    {
        leftArmRenderer.enabled = true;
    }

    public void hideLeftArm()
    {
        leftArmRenderer.enabled = false;
    }

    public void showRightArm()
    {
        rightArmRenderer.enabled = true;
    }

    public void hideRightArm()
    {
        rightArmRenderer.enabled = false;
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

    // Animation Events

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
    public void punchImpact()
    {
    }
}
