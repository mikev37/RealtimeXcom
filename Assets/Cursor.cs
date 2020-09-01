using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    public GameObject bullet;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    float timer;
    // Update is called once per frame
    void Update()
    {
        Vector3 screenPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit hit;
        Debug.DrawRay(screenPosition, Camera.main.transform.forward);

        Ray ray = new Ray();
        ray.origin = screenPosition;
        ray.direction = Camera.main.transform.forward;

        if (Physics.Raycast(ray, out hit))
        {
            transform.position = hit.point;
        }

        if (Input.GetMouseButton(1) && timer <= 0)
        {
            GameObject.Instantiate(bullet, ray.origin, Quaternion.LookRotation(ray.direction) * (Quaternion.RotateTowards(Quaternion.identity,Random.rotation,2)) );
            timer = .05f;
        }
        else timer -= Time.deltaTime;
    }
}
