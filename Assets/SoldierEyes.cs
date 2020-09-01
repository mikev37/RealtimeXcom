using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierEyes : MonoBehaviour
{
    public List<GameObject> seen;
    public List<GameObject> inConeSeen;
    private void Start()
    {
        seen = new List<GameObject>();
        inConeSeen = new List<GameObject>();
    }
    public LayerMask mask;

    public bool See(GameObject gol)
    {
        GameObject go = gol.GetComponentInChildren<CenterMass>().gameObject;
        Debug.DrawRay(transform.position, go.transform.position - transform.position + UnityEngine.Random.insideUnitSphere*3);
        RaycastHit hit;
        for (int i = 0; i < 5; i++)
        {
            if (Physics.Raycast(transform.position, go.transform.position - transform.position + UnityEngine.Random.insideUnitSphere * 3, out hit, 1000, mask.value, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawLine(transform.position, hit.point, Color.red);
                if (hit.collider.attachedRigidbody && hit.collider.attachedRigidbody.gameObject.Equals(gol))
                    return true;
            }
        }
        return false;
    }

    public void Update()
    {
        inConeSeen.RemoveAll(x => x.GetComponentInParent<Agent>() == null || x.GetComponentInParent<Agent>().enabled == false);
        seen.Clear();
        seen.AddRange(inConeSeen.FindAll(x => See(x)));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody && 
            !inConeSeen.Contains(other.attachedRigidbody.gameObject) && 
            other.attachedRigidbody.gameObject.layer.Equals(LayerMask.NameToLayer("Soldier"))&&
            other.attachedRigidbody.GetComponent<Agent>())
        {
            Ray ray = new Ray(transform.position, other.attachedRigidbody.transform.position - transform.position);
            inConeSeen.Add(other.attachedRigidbody.gameObject);
            Debug.Log("Target Seen");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
        if (other.attachedRigidbody && 
            inConeSeen.Contains(other.attachedRigidbody.gameObject) && 
            other.attachedRigidbody.gameObject.layer.Equals(LayerMask.NameToLayer("Soldier")))
        {
            inConeSeen.Remove(other.attachedRigidbody.gameObject);
            Debug.Log("Target Lost");
        }
        
    }

    internal bool Blocked()
    {
        return Physics.Raycast(transform.position, transform.forward, 10, mask.value, QueryTriggerInteraction.Ignore);
    }
}
