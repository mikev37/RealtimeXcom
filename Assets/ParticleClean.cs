using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleClean : MonoBehaviour
{
    public ParticleSystem ps;
    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        //ps.main.loop = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(ps.main.ps.time);
        if(ps.time >= ps.main.duration)
        {
            Destroy(gameObject);
        }
    }
}
