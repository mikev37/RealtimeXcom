using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{

    public Animation shoot;

    public int ammo;
    public int ammoMax;

    public GameObject projectile;

    public float timer, shotTime, loadTime;

    public Transform barrel;

    public AudioSource load;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool loading;

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        { 
            timer = 0;
            if (loading)
            {
                ammo = ammoMax;
                loading = false;
            }
        }

    }

    public void Fire(float accuracy)
    {
        if (ammo > 0 && timer <= 0 && !loading)
        { 
            GameObject.Instantiate(projectile, barrel.position, barrel.rotation * (Quaternion.RotateTowards(Quaternion.identity, UnityEngine.Random.rotation, 300 / accuracy)), null);
            ammo--;
            timer = shotTime;
        }
    }

    public void Load()
    {
        if (!loading)
        {
            loading = true;
            timer = loadTime;
            load.Play();
        }

    }
}
