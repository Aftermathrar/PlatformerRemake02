using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{

    public float minGroundNormalY = .65f;
    public float gravityModifier = 2f;
    public LayerMask groundMask;

    protected Vector2 targetVelocity;
    protected bool grounded;
    protected bool hittingWall;
    protected Vector2 groundNormal;
    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected RaycastHit2D[] hitBufferBack = new RaycastHit2D[16];
    protected RaycastHit2D[] hitBufferFront = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

    protected BoxCollider2D hitbox;
    protected float hitboxScale;
    protected float hitboxOffsetX;
    protected float hitboxWidth;
    protected float hitboxOffsetY;
    protected float hitboxHeight;

    // protected const float minMoveDistance = 0.0001f;                            // 144Hz settings
    // protected const float shellRadius = 0.003472222f;                           // 144Hz settings

    protected float minMoveDistance;
    protected float shellRadius;

    void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();

        // Calculate minimum move distance based on gravity and framerate, mostly used for Y movement collision
        minMoveDistance = Mathf.Abs(Physics2D.gravity.y * Time.fixedDeltaTime * Time.fixedDeltaTime / gravityModifier / 2);
        shellRadius = minMoveDistance * 15;

        grounded = false;

        hitbox = GetComponent<BoxCollider2D>();
        hitboxScale = rb2d.transform.localScale.x;
        hitboxOffsetX = (hitbox.offset.x) * hitboxScale;
        hitboxWidth = (hitbox.size.x / 2) * hitboxScale;
        hitboxOffsetY = (hitbox.offset.y - hitbox.size.y / 2) * hitboxScale;
        // hitboxHeight = (hitbox.size.y / 2) * hitboxScale;


    }

    void Start()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
        groundNormal = new Vector2(0, 1);
    }

    void Update()
    {
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {

    }

    void FixedUpdate()
    {
        velocity += gravityModifier * Physics2D.gravity * Time.fixedDeltaTime;
        velocity.x = targetVelocity.x;

        Vector2 deltaPosition = velocity * Time.deltaTime;

        //if (groundNormal.y < 0)
        //    groundNormal = Vector2.up;

        // Debug.Log("currentNormal: " + groundNormal);

        //if (groundNormal.y > 0 && ((velocity.x < 0 && groundNormal.x < 0) || (velocity.x > 0 && groundNormal.x > 0)))
        //    groundNormal = Vector2.up;

        Debug.DrawRay(rb2d.position, groundNormal);
        
        // Set ground movement vector perpendicular to ground slope vector
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

        // Reset wall check
        hittingWall = false;
        
        Vector2 move = moveAlongGround * deltaPosition.x;

        Movement(move, false);

        move = Vector2.up * deltaPosition.y;
        
        Movement(move, true);

        // Debug.Log(velocity.x);
    }

    void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;
        
        if (distance > minMoveDistance)
        {
            int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
            hitBufferList.Clear();
            
            float hitboxSkinX = hitboxOffsetX + Mathf.Sign(-targetVelocity.x) * (hitboxWidth - .01f);

            if (!yMovement)
            {
                Vector2 kneePos = new Vector2(rb2d.position.x + hitboxOffsetX + Mathf.Sign(targetVelocity.x)*hitboxWidth, rb2d.position.y + hitboxOffsetY + .1f);
                RaycastHit2D wallCheck = Physics2D.Raycast(kneePos, Vector2.right * Mathf.Sign(move.x), .05f + distance, groundMask);

                if (wallCheck.collider != null)
                {
                    if (wallCheck.normal.y == 0)
                        hittingWall = true;
                }
                Debug.DrawRay(kneePos, Vector2.right * Mathf.Sign(move.x), Color.red);
            }


            if (count == 0 && yMovement)
            {
                groundNormal.y = 1;
                groundNormal.x = 0;
            }

            bool onSlope = false;

            // Attempting to check for slopes and move to the ground if heading down an embankment
            if (yMovement && velocity.y <= 0)
            {
                grounded = false;

                Vector2 hitboxPosCorner = new Vector2(rb2d.position.x + hitboxSkinX, rb2d.position.y + hitboxOffsetY + shellRadius);
                Vector2 hitboxPosOppCorner = new Vector2(rb2d.position.x + hitboxOffsetX + Mathf.Sign(targetVelocity.x) * (hitboxWidth / 2), rb2d.position.y + hitboxOffsetY + shellRadius);

                var slopeCheckDistance = (Mathf.Abs(targetVelocity.x * Time.fixedDeltaTime / hitboxScale) + shellRadius);

                int countBack = Physics2D.Raycast(hitboxPosCorner, Vector2.down, contactFilter, hitBufferBack, slopeCheckDistance);
                int countFront = Physics2D.Raycast(hitboxPosOppCorner, Vector2.down, contactFilter, hitBufferFront, slopeCheckDistance);
                // Debug.Log(Mathf.Abs(targetVelocity.x * Time.fixedDeltaTime));
                // Debug.Log(move);

                Debug.DrawRay(hitboxPosCorner, Vector2.down * slopeCheckDistance, Color.cyan);
                Debug.DrawRay(hitboxPosOppCorner, Vector2.down * slopeCheckDistance, Color.green);
                if (countBack > 0 && countFront == 0 && hitBufferBack[0].normal.y >= minGroundNormalY)
                {
                    distance += -(hitBufferBack[0].point.y - hitboxPosCorner.y - shellRadius);

                    grounded = true;
                    onSlope = true;
                }
                else if(countFront > 0 && countBack == 0 && hitBufferFront[0].normal.y == 1)
                {
                    onSlope = true;
                }
                //else if(countBack > 0 && countFront == 0 && hitBufferBack[0].normal.y < minGroundNormalY)
                //{
                // TODO: make character slide
                // Vector2 slideDistance = new Vector2(velocity.x * Time.fixedDeltaTime * hitBufferBack[0].normal.y, velocity.y * Time.fixedDeltaTime * hitBufferBack[0].normal.x);
                // rb2d.position = new Vector2(rb2d.position.x + slideDistance.x, rb2d.position.y + slideDistance.y);
                // velocity *= slideDistance;
                // }
            }

            for (int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            for (int i = 0; i < hitBufferList.Count; i++)
            {
                Vector2 currentNormal = hitBufferList[i].normal;
                // Debug.Log(hitBufferList[i].distance);
                if (currentNormal.y > minGroundNormalY)
                {
                    grounded = true;

                    if (yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;

                        float projection = Vector2.Dot(velocity, currentNormal);
                        
                        if (projection < 0)
                        {
                            velocity -= projection * currentNormal;
                        }

                    }

                }

                // Get actual distance to collision, angle of collision, and direction of movement
                float modifiedDistance = hitBufferList[i].distance - shellRadius;
                float colAngle = Vector2.Angle(hitBufferList[i].normal, Vector2.up);
                float leftRight = Mathf.Sign(move.x);

                if (colAngle > 90 && yMovement && !grounded)
                {
                    // if jumping into a ceiling, bonk away from the slope

                    rb2d.position += new Vector2(distance * hitBufferList[i].normal.x, -0.01f);

                    // reset vertical velocity if moving upward and start to fall
                    velocity.y = velocity.y > 0 ? -0.13625f : velocity.y;
                }
                else if (colAngle >= 90 || (hittingWall && !yMovement))
                {
                    // If hitting wall, stop sideways movement
                    move *= Vector2.up;
                    distance = modifiedDistance < distance ? 0 : distance;
                    Debug.Log("Hitting wall.");
                }
                else if (grounded && colAngle < 90 && !yMovement)
                {
                    // ground slope movement, should slow player down slightly when climbing
                    move = move.magnitude * new Vector2(leftRight * hitBufferList[i].normal.y, -leftRight * hitBufferList[i].normal.x);
                    
                    if (leftRight == Mathf.Sign(hitBufferList[i].normal.x))
                        distance = distance / hitBufferList[i].normal.y;

                }
                else if (grounded && yMovement && velocity.y > 0)
                {
                    // Don't modify distance so that character can escape from ground
                    grounded = false;
                    // Debug.Log("Stuck in ground");
                }
                else if (yMovement && colAngle == 0 && !onSlope)
                {
                    // If on flat ground, modify y position to match ground
                    distance = 0;
                    Vector2 hitPoint = hitBufferList[i].point;
                    rb2d.position = new Vector2(rb2d.position.x, hitPoint.y);
                    grounded = true;
                }
                else if (yMovement)
                {
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }

            }


        }
        
        rb2d.position = rb2d.position + move.normalized * distance;
        


        // Re-calculate hitbox offset
        hitboxOffsetX = (hitbox.offset.x) * hitboxScale;

        // Check bottom left and right corners of hitbox for collision below to see when we're no longer grounded
        // Should be able to write a slope check with previous grounded state to stop falling down slopes
        //Vector2 footPosLeft = new Vector2(rb2d.position.x + hitboxOffsetX - hitboxWidth, rb2d.position.y + hitboxOffsetY + 0.05f);
        //Vector2 footPosRight = new Vector2(rb2d.position.x + hitboxOffsetX + hitboxWidth, rb2d.position.y + hitboxOffsetY + 0.05f);
        //int floorFoundLeft = Physics2D.Raycast(footPosLeft, -Vector2.up, contactFilter, footCheckLeft, 0.1f + shellRadius);
        //int floorFoundRight = Physics2D.Raycast(footPosRight, -Vector2.up, contactFilter, footCheckRight, 0.1f + shellRadius);

        //// Debug.DrawRay(footPosLeft, Vector2.down, Color.red);
        //// Debug.DrawRay(footPosRight, Vector2.down, Color.green);

        //// Debug.Log(footCheckLeft[0].collider.name);

        //if (floorFoundLeft + floorFoundRight == 0)
        //{
        //    // grounded = false;
        //}
    }

}
