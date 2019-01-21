using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private Text _text;
    private float timer = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<Text>();
        _text.text = "0";
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
            timer = 0f;
        timer += Time.deltaTime;
        _text.text = timer.ToString();
    }
}
