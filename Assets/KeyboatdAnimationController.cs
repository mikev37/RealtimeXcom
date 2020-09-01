using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboatdAnimationController : MonoBehaviour
{
    public Animator legs;
    public Animator body;
    public CreateRagDoll rags;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            legs.ResetTrigger("Center");
            legs.ResetTrigger("Right");
            legs.SetTrigger("Left");
        }
        else if (Input.GetKey(KeyCode.D))
        {
            legs.ResetTrigger("Center");
            legs.ResetTrigger("Left");
            legs.SetTrigger("Right");
        }
        else
        {
            legs.ResetTrigger("Left");
            legs.ResetTrigger("Right");
            legs.SetTrigger("Center");
        }


        if (Input.GetKey(KeyCode.W))
        {
            legs.ResetTrigger("Jump");
            legs.ResetTrigger("Idle");
            legs.SetTrigger("Run");
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            legs.ResetTrigger("Run");
            legs.ResetTrigger("Idle");
            legs.SetTrigger("Jump");
        }
        else
        {
            legs.ResetTrigger("Run");
            legs.ResetTrigger("Jump");
            legs.SetTrigger("Idle");
        }



        if (Input.GetKey(KeyCode.LeftControl))
        {
            legs.ResetTrigger("Stand");
            legs.SetTrigger("Crouch");
        }
        else
        {
            legs.ResetTrigger("Crouch");
            legs.SetTrigger("Stand");
        }



        if (Input.GetKey(KeyCode.R))
        {
            body.ResetTrigger("Idle");
            body.ResetTrigger("Fire");
            body.ResetTrigger("Aim");
            body.SetTrigger("Reload");
        }
        else if (Input.GetMouseButton(0))
        {
            body.ResetTrigger("Idle");
            body.ResetTrigger("Reload");
            body.ResetTrigger("Aim");
            body.SetTrigger("Fire");
        }
        else if (Input.GetMouseButton(1))
        {
            body.ResetTrigger("Idle");
            body.ResetTrigger("Fire");
            body.ResetTrigger("Reload");
            body.SetTrigger("Aim");
        }
        else
        {
            body.ResetTrigger("Aim");
            body.ResetTrigger("Fire");
            body.ResetTrigger("Reload");
            body.SetTrigger("Idle");
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            body.enabled = false;
            legs.enabled = false;
            rags.Break();
        }
    }
}
