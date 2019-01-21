using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    private Animator anim;                  // Animator caching
    private CharacterPhysics _controller;   // Physics controller
    private float timeIASA = .667f;         // Interruptable as soon as frame
    private float attackTime = 0f;          // Tracks attack animation time

    private bool attackBuffer = false;      // Buffer inputs slightly for repeated attacks
    private bool groundAttack = true;       // Used to determine if it's an air attack
    private bool isGrounded = true;         // Check whether the play is grounded

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("No animator found for Attack Script");
        }

        _controller = GetComponent<CharacterPhysics>();
        if (_controller == null)
        {
            Debug.LogError("No physics controller found for Attack Script");
        }
    }

    void Update()
    {

        if ((Input.GetButtonDown("Fire1") || attackBuffer) && attackTime == 0f)
        {
            attackTime = timeIASA;
            anim.SetBool("Attack", true);
            anim.SetFloat("AttackTimer", attackTime);
            attackBuffer = false;

            if (isGrounded)
            {
                groundAttack = true;
            }
            else
            {
                groundAttack = false;
            }
        }
        else if (Input.GetButtonDown("Fire1") && attackTime < 0.3f)
        {
            Debug.Log("Buffered Attack");
            attackBuffer = true;
        }
        else if (attackTime > 0)
        {
            attackTime -= Time.deltaTime;
            if (attackTime < 0)
            {
                attackTime = 0f;
            }
        }
        anim.SetFloat("AttackTimer", attackTime);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        isGrounded = anim.GetBool("Grounded");

        // Remove attack boolean after a short period to help with buffering attacks
        if (attackTime <= 0.5f)
            anim.SetBool("Attack", false);

        if (!groundAttack && isGrounded)
        {
            attackTime = 0f;
            anim.SetBool("Attack", false);
        }
        //else if (groundAttack && !isGrounded)
        //{
        //    // Bad physics, letting ground attacks happen in the air
        //    attackTime = 0f;
        //    anim.SetBool("Attack", false);
        //}
    }
}
