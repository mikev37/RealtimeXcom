using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBody : MonoBehaviour
{

    private void Start()
    {
    }
    
    //perform rigor mortis and then delete everything for performance reasons
    private void Update()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.drag += Time.deltaTime;//.isKinematic = true;
        rigidbody.angularDrag += Time.deltaTime / 10f;
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.drag += Time.deltaTime;//.isKinematic = true;
            rb.angularDrag += Time.deltaTime / 10f;
        }
        if (rigidbody.drag > 15)
        {
            foreach (Collider cj in GetComponentsInChildren<Collider>())
            {
                Destroy(cj);
            }
            foreach (Joint cj in GetComponentsInChildren<Joint>())
            {
                Destroy(cj);
            }
            foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
            {
                Destroy(rb);
            }
            Destroy(this);
        }
    }
    
}
