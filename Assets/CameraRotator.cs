﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            transform.rotation *= Quaternion.Euler(0, 90, 0);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            transform.rotation *= Quaternion.Euler(0, 90, 0);
        }
    }
}
