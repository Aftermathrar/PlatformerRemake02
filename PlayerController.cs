using UnityEngine;
using System.Collections;
using Prime31;


public class PlayerController : MonoBehaviour
{
    //Other movement config
    public float maxSpeed = 8f;                                     // Max run speed
    // public float acceleration = 10f;                             //TODO: find out how to change input axis sensitivity/gravity
    public float gravityModifier = 2f;
    public float jumpHeight = 14f;                                  // used with double gravity
    public float doubleJumpMultiplier = 0.8f;                       // Double jump has slightly less height, multiplier for jump speed
    [Range(1.5f, 3f)] public float jumpKickSpeedX = 2.0f;           // Extra speed when jumpkicking
    [Range(-10f, -20f)] public float jumpKickSpeedY = -15f;         // Downward velocity when starting a jumpkick
    [Range(1f, 2f)] public float slideKickSpeedX = 1.5f;            // Extra speed when sliding

    private bool hasDoubleJump = true;
    private bool usedJumpKick = false;
    private bool usedGroundAttack = false;
    private bool crouch = false;

    private float _attackTimer = 0f;
    private float jumpKickMove;                             // Saves movement at the start of the jumpkick
    private float slideKick = 0.833f;
    private float slideKickTimer = 0f;
    private float slideKickMove;                            // Saves movement at the start of the slide

    [HideInInspector]
	private float normalizedHorizontalSpeed = 0;
    private float verticalAxis = 0;
    public static bool jumpThisFrame = false;
    [Range(0, 0.5f)]
    public float oneWayPlatformTimer = 0.1f;
    private float oneWayTime;

	private CharacterPhysics _controller;
	private Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector2 _velocity;

    // Cache stuff for flipping sprite
    private Rigidbody2D _spriteBody;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _hitBoxFlip;


    void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<CharacterPhysics>();
        _spriteBody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _hitBoxFlip = GetComponent<BoxCollider2D>();

        // listen to some events for illustration purposes
        _controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}


	#region Event Listeners

	void onControllerCollider( RaycastHit2D hit )
	{
		// bail out on plain old ground hits cause they arent very interesting
		if( hit.normal.y == 1f )
			return;

		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		//Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
	}


	void onTriggerEnterEvent( Collider2D col )
	{
		Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );
	}


	void onTriggerExitEvent( Collider2D col )
	{
		Debug.Log( "onTriggerExitEvent: " + col.gameObject.name );
	}

	#endregion


	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update()
	{
		if( _controller.isGrounded)
        {
            _animator.SetBool("Grounded", true);
            _velocity.y = 0;
        } else
        {
            _animator.SetBool("Grounded", false);
        }

        normalizedHorizontalSpeed = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");

        _attackTimer = _animator.GetFloat("AttackTimer");

        // Reset jump state
        jumpThisFrame = false;

        // Reset crouch state
        crouch = false;
        
        if (_attackTimer > 0 && _controller.isGrounded)
        {
            usedGroundAttack = true;
        }
        else
        {
            usedGroundAttack = false;
        }

        // Normal jump if hitting jump, on the ground, not attacking, and not holding down
        if (Input.GetButtonDown("Jump") && _controller.isGrounded && _attackTimer == 0 && verticalAxis >= 0)
        {
            // _controller.isGrounded = false;
            _velocity.y = jumpHeight;
            jumpThisFrame = true;
        } // Double jump if hitting jump, not on the ground, not attacking, and has double jump
        else if (Input.GetButtonDown("Jump") && !_controller.isGrounded && hasDoubleJump && _attackTimer == 0)
        {
            //Set anim and flag for double jump
            _animator.SetBool("DoubleJump", hasDoubleJump);
            hasDoubleJump = false;
            jumpThisFrame = true;

            //Double jump has slightly less height
            _velocity.y = jumpHeight * doubleJumpMultiplier;
        } // If letting go of jump while moving upward, reduce speed for shorter jump
        else if (Input.GetButtonUp("Jump"))
        {
            if (_velocity.y > 0)
            {
                //velocity.y = velocity.y * 0.5f;
                _velocity.y *= 0.25f;
            }
        } // If holding down, hitting jump, not attacking, and have used a double jump but not jump kick, use jump kick
        else if (verticalAxis < 0 && Input.GetButtonDown("Jump") && _animator.GetBool("DoubleJump") && !usedJumpKick && _attackTimer == 0)
        {
            // usedJumpKick and jumpKickMove locks your movement, taking away control until you land
            usedJumpKick = true;
            jumpKickMove = normalizedHorizontalSpeed;
            _animator.SetBool("JumpKick", usedJumpKick);
        } // If on the ground and holding down, crouch
        else if(_controller.isGrounded && verticalAxis < 0)
        {
            crouch = true;
            _animator.SetBool("Crouch", crouch);
        }

        // If crouched, hitting jump, and not on cd from sliding or attacking, use slide kick
        if (crouch && Input.GetButtonDown("Jump") && slideKickTimer <= 0 && _attackTimer == 0)
        {
            slideKickTimer = slideKick;
            _animator.SetFloat("Slide", slideKickTimer);

            // If facing right, move right, otherwise go left
            if (_spriteRenderer.flipX)
                slideKickMove = -1f;
            else
                slideKickMove = 1f;

        } // If using SlideKick, decrement timer. If in the air, stop the slide kick
        else if(slideKickTimer > 0)
        {
            if (_controller.isGrounded)
                slideKickTimer -= Time.deltaTime;
            else
                slideKickTimer = 0;
            _animator.SetFloat("Slide", slideKickTimer);
        }

        // If able to change directions, flip sprite
        if (_attackTimer == 0 && !usedJumpKick && slideKickTimer <= 0)
        {
            bool flipSprite = (_spriteRenderer.flipX ? (normalizedHorizontalSpeed > 0.01f) : (normalizedHorizontalSpeed < -0.01f));
            if (flipSprite)
            {
                // Flip sprite rotation
                _spriteRenderer.flipX = !_spriteRenderer.flipX;

                // If hitbox is offset, calculate swap
                //_hitBoxFlip.offset = new Vector2(_hitBoxFlip.offset.x * -1, _hitBoxFlip.offset.y);
                
                // move the sprite body when flipping if the hitbox is not centered on X
                //_spriteBody.position = new Vector2(_spriteBody.position.x + (Mathf.Abs(_hitBoxFlip.offset.x) * 2 * Mathf.Sign(normalizedHorizontalSpeed)), _spriteBody.position.y);
            }
        }

        // if holding down bump up our movement amount and turn off one way platform detection for a small window.
        // this lets us jump down through one way platforms
        if (_controller.isGrounded && Input.GetAxisRaw("Vertical") < 0 && Input.GetButtonDown("Jump"))
        {
            _velocity.y *= 3f;
            oneWayTime = oneWayPlatformTimer;
        }

        if (oneWayTime > 0)
        {
            oneWayTime -= Time.deltaTime;

            _controller.ignoreOneWayPlatformsThisFrame = true;
        }



        // apply gravity before moving
        _velocity += Physics2D.gravity * Time.deltaTime * gravityModifier;
        
        if (jumpThisFrame)
            _animator.SetBool("Grounded", false);
        _animator.SetFloat("VelocityX", Mathf.Abs(_velocity.x) / maxSpeed);
        _animator.SetFloat("VelocityY", _velocity.y);
        _animator.SetBool("Crouch", crouch);

        // Reset double jump if landed
        if (_controller.isGrounded && !hasDoubleJump)
        {
            _animator.SetBool("DoubleJump", hasDoubleJump);
            hasDoubleJump = true;
            usedJumpKick = false;
            _animator.SetBool("JumpKick", usedJumpKick);
        }

        // Set velocity based on animation state

        // If using ground attack, stop horizontal movement
        if (usedGroundAttack)
        {
            _velocity *= Vector2.up;
        } // If using jumpkick, stick to the static movement
        else if (usedJumpKick)
        {
            _velocity = new Vector2(jumpKickMove * maxSpeed * jumpKickSpeedX, _velocity.y);
            //if (_velocity.y >= jumpKickSpeedY)
            //{
                _velocity = new Vector2(_velocity.x, jumpKickSpeedY);
            //}
        } // If using slide kick, lock velocity based on timer of slide
        else if(slideKickTimer > 0)
        {
            float slideMovement = slideKickMove * maxSpeed * slideKickSpeedX * slideKickTimer;
            _velocity = new Vector2(slideMovement, _velocity.y);
        } // If crouched, don't allow horizontal movement
        else if (crouch)
        {
            _velocity *= Vector2.up;
        }
        else
        {
            _velocity = new Vector2(normalizedHorizontalSpeed * maxSpeed, _velocity.y);
        }

        _controller.move( _velocity * Time.deltaTime );

		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;

	}

}
