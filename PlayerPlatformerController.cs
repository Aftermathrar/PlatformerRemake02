using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicsObject))]
public class PlayerPlatformerController : PhysicsObject
{

    public float maxSpeed = 8f;                             // Max run speed
    public float acceleration = 10f;                        //TODO: find out how to change input axis sensitivity/gravity
    public float jumpTakeOffSpeed = 10f;                    // used with double gravity
    public float doubleJumpTakeOffMultiplier = 0.8f;        // Double jump has slightly less height, multiplier for jump speed
    private float jumpKickSpeedX = 2.0f;                    // Extra speed when jumpkicking
    private float jumpKickSpeedY = -15f;                    // Downward velocity when starting a jumpkick

    private bool hasDoubleJump = true;
    private bool usedJumpKick = false;
    private bool usedGroundAttack = false;

    private float _attackTimer = 0f;
    private Vector2 jumpKickMove;                   // Saves movement at the start of the jumpkick

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BoxCollider2D hitBoxFlip;
    private Rigidbody2D spriteBody;


    [SerializeField]
    // private StatusIndicator statusIndicator;

    // Use this for initialization
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        hitBoxFlip = GetComponent<BoxCollider2D>();
        spriteBody = GetComponent<Rigidbody2D>();
        // firePoint = transform.Find("FirePoint");
    }

    //void Start()
    //{
    //    GameMaster.gm.onToggleUpgradeMenu += OnUpgradeMenuToggle;
    //}

    //void OnUpgradeMenuToggle(bool active)
    //{
    //    // Handle what happens when the upgrade menu is toggled
    //    GetComponent<PhysicsObject>().enabled = !active;
    //}

    protected override void ComputeVelocity()
    {
        Vector2 move = Vector2.zero;

        move.x = Input.GetAxisRaw("Horizontal");

        _attackTimer = animator.GetFloat("AttackTimer");

        if (_attackTimer > 0 && grounded)
        {
            usedGroundAttack = true;
        }
        else
        {
            usedGroundAttack = false;
        }

        if (Input.GetButtonDown("Jump") && grounded && _attackTimer == 0)
        {
            grounded = false;
            velocity.y = jumpTakeOffSpeed;
            groundNormal.y = 1;
            groundNormal.x = 0;
        }
        else if (Input.GetButtonDown("Jump") && hasDoubleJump && _attackTimer == 0)
        {
            //Set anim and flag for double jump
            animator.SetBool("DoubleJump", hasDoubleJump);
            hasDoubleJump = false;
            
            //Double jump has slightly less height
            velocity.y = jumpTakeOffSpeed * doubleJumpTakeOffMultiplier;
        }
        else if (Input.GetButtonUp("Jump"))
        {
            if (velocity.y > 0)
            {
                //velocity.y = velocity.y * 0.5f;
                velocity.y = 0;
            }
        }
        else if (Input.GetAxisRaw("Vertical") < 0 && Input.GetButtonDown("Jump") && animator.GetBool("DoubleJump") && !usedJumpKick && _attackTimer == 0)
        {
            usedJumpKick = true;
            jumpKickMove = move;
            animator.SetBool("JumpKick", usedJumpKick);
        }

        if (_attackTimer == 0 && !usedJumpKick)
        {
            bool flipSprite = (spriteRenderer.flipX ? (move.x > 0.01f) : (move.x < -0.01f));
            if (flipSprite)
            {
                // Flip sprite rotation
                spriteRenderer.flipX = !spriteRenderer.flipX;

                // Archer hitbox is offset, calculate swap
                hitBoxFlip.offset = new Vector2(hitBoxFlip.offset.x * -1, hitBoxFlip.offset.y);
                spriteBody.position = new Vector2(spriteBody.position.x + (Mathf.Abs(hitboxOffsetX) * 2 * Mathf.Sign(move.x)), spriteBody.position.y);
            }
        }

        animator.SetBool("Grounded", grounded);
        animator.SetFloat("VelocityX", Mathf.Abs(velocity.x) / maxSpeed);
        animator.SetFloat("VelocityY", velocity.y);

        if (grounded && !hasDoubleJump)
        {
            animator.SetBool("DoubleJump", hasDoubleJump);
            hasDoubleJump = true;
            usedJumpKick = false;
            animator.SetBool("JumpKick", usedJumpKick);
        }

        if (usedGroundAttack)
        {
            targetVelocity *= Vector2.up;
        }
        else if (usedJumpKick)
        {
            targetVelocity = jumpKickMove * maxSpeed * jumpKickSpeedX;
            if (velocity.y >= jumpKickSpeedY)
            {
                velocity = new Vector2(velocity.x, jumpKickSpeedY);
            }
        }
        else
        {
            targetVelocity = move * maxSpeed;
        }
        // statusIndicator.SetHealth(Mathf.RoundToInt(Mathf.Abs(velocity.x)));
        // statusIndicator.SetHealth(AttackScript.instance.attackCooldown);
    }
}