using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suppression : MonoBehaviour
{
    public int suppression;
    public Vector3 origin;
    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody)
        {
            Agent s = other.attachedRigidbody.gameObject.GetComponent<Agent>();
            if (s && !s.takingFire.Contains(origin) && Vector3.Distance(s.gun.barrel.transform.position,origin) > 5)
            {
                s.takingFire.Add(origin);
                if (s.takingFire.Count > 5)
                    s.takingFire.RemoveAt(0);
                s.morale -= suppression * Mathf.Max(0, (GetComponent<Collider>().bounds.size.x - Vector3.Distance(s.transform.position, transform.position)));
            }
        }
    }
}
