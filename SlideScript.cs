using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideScript : MonoBehaviour
{
    private SpriteRenderer _sprite;
    
    void Awake()
    {
        _sprite = GetComponentInParent<SpriteRenderer>();
    }

    void OnEnable()
    {
        // If sprite is flipped, reverse local scale to change offset
        if (_sprite.flipX)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
