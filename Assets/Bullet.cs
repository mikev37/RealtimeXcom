using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject puff;
    public LayerMask mask;
    public Damage d;
    // Start is called before the first frame update
    void Start()
    {
        RaycastHit hit;
        Ray ray = new Ray();
        ray.origin = transform.position;
        ray.direction = transform.forward;

        if (Physics.Raycast(ray, out hit, 10000, mask.value, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(ray.origin, ray.direction * 10000);
            GameObject go = GameObject.Instantiate(puff, hit.point, Quaternion.Inverse(transform.rotation));
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            Suppression sup = go.GetComponent<Suppression> ();
            sup.origin = transform.position;
            if (hit.collider.attachedRigidbody)
            {
                Shootable s = hit.collider.attachedRigidbody.gameObject.GetComponent<Shootable>();
                if (s != null)
                {
                    s.getShot(d);
                    ParticleSystem.ColorOverLifetimeModule coltm = ps.colorOverLifetime;
                    coltm.color = Color.red;
                }
            }

        }
    }
}
