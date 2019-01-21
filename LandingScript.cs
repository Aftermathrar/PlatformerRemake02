using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingScript : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _sprite;

    private CircleCollider2D _col;
    public LayerMask whatIsGround;

    // Start is called before the first frame update
    void Awake()
    {
        _animator = GetComponentInParent<Animator>();
        _sprite = GetComponentInParent<SpriteRenderer>();
        _col = GetComponent<CircleCollider2D>();
    }

    void OnEnable()
    {
        if (_sprite.flipX)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        } else
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    //void FixedUpdate()
    //{
    //    if (Physics2D.OverlapCircle(_col.transform.position, _col.radius, whatIsGround))
    //    {
    //        Debug.Log("Physics2D: ");
    //        //if (other.CompareTag("Platform") || other.CompareTag("OneWayPlatform"))
    //        //{
    //            _animator.SetBool("Grounded", true);
    //            _animator.SetBool("JumpKick", false);
    //        //}
    //    }
    //}

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger: " + other.tag);
        if (other.CompareTag("Platform") || other.CompareTag("OneWayPlatform"))
        {
            _animator.SetBool("Grounded", true);
            _animator.SetBool("JumpKick", false);
        }
    }


    void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Collision: " + other.collider.tag);
        if (other.collider.CompareTag("Platform") || other.collider.CompareTag("OneWayPlatform"))
        {
            _animator.SetBool("Grounded", true);
            _animator.SetBool("JumpKick", false);
        }
    }
}
