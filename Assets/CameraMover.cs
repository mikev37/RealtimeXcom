using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Input.GetAxis("Vertical") * transform.forward + Input.GetAxis("Horizontal") * transform.right;
    }
}
