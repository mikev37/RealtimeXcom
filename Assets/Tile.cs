using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Tile : MonoBehaviour
{
    public Dictionary<Tile,int> neighbors;
    public int F,x,y;
    public string debug;
    public LayerMask mask;

    public Agent occupied;
    // Start is called before the first frame update
    void Start()
    {

        if (Physics.BoxCast(transform.position + Vector3.up * 20, Vector3.one * 3, Vector3.down, Quaternion.identity, 30, mask.value, QueryTriggerInteraction.Ignore))
        {
            foreach (Tile t in neighbors.Keys.ToList())
            {
                neighbors[t] = 1000;
                t.neighbors[this] = 1000;
            }
        }
        Ray ray = new Ray(transform.position + Vector3.up * 6, Vector3.right * 10);

        foreach (Tile n in neighbors.Keys.ToList())
        {
            ray.origin = transform.position + Vector3.up * 6;
            ray.direction = n.transform.position + Vector3.up * 6 - ray.origin;
            Color c = Color.green;
            if (Physics.Raycast(ray, 10, mask.value, QueryTriggerInteraction.Ignore))
            {
                n.neighbors[this] = 50;
                neighbors[n] = 50;
            }
            ray.origin = transform.position + Vector3.up * 12;
            ray.direction = n.transform.position + Vector3.up * 12 - ray.origin;
            if (Physics.Raycast(ray, 10, mask.value, QueryTriggerInteraction.Ignore))
            { 
                n.neighbors[this] = 1000;
                neighbors[n] = 1000;
            }
        }
    }


    public int getCoverValue(Vector3 shooterOrigin)
    {
        shooterOrigin.y = 0;
        float minAngle = float.MaxValue;
        //Tile relevant = null;
        int value = 0;
        foreach(Tile n in neighbors.Keys)
        {
            Vector3 toShooter = shooterOrigin - transform.position;
            Vector3 toNeighbor = n.transform.position - transform.position;
            float angle = Vector3.Angle(toShooter, toNeighbor);
            if (angle < 0)
                throw new System.Exception("FUCK");
            if (angle < 45)
            {
                int bonus = 1;
                int dir = 0;
                if (angle < 25)
                    bonus = 2;
                if (n.neighbors[this] > 500)
                    dir = 2;
                else if (n.neighbors[this] > 00)
                    dir = 1;

                value += dir * bonus;
                if(dir > 0)
                    Debug.DrawRay(transform.position, toNeighbor* .5f, Color.cyan, 1);
                if (bonus > 1 && dir > 0)
                    Debug.DrawRay(transform.position, toNeighbor * .7f, Color.white, 1);
            }
        }
        
        return value;
    }

    private void OnCollisionEnter(Collision collision)
    {
    }
    
    // Update is called once per frame
    void Update()
    {

        
        Ray ray = new Ray(transform.position + Vector3.up * 6, Vector3.right * 10);

        foreach (Tile n in neighbors.Keys)
        {
            ray.origin = transform.position + Vector3.up * 6;
            ray.direction = n.transform.position + Vector3.up * 6 - ray.origin;
            Color c = Color.green;
            if (Physics.Raycast(ray, 10, mask.value, QueryTriggerInteraction.Ignore)) { c = Color.red; } else c = Color.green;
            Debug.DrawRay(ray.origin, ray.direction, c);
            ray.origin = transform.position + Vector3.up * 12;
            ray.direction = n.transform.position + Vector3.up * 12 - ray.origin;
            if (Physics.Raycast(ray, 10, mask.value, QueryTriggerInteraction.Ignore)) { c = Color.red; } else c = Color.green;
            Debug.DrawRay(ray.origin, ray.direction, c);

        }
    }

    internal bool Obstructed(Vector3 forward)
    {
        return neighbors[TileGenerator.tg.t[x, y-1]] > 0;

        return false;
    }

    public void OnMouseOver()
    {
       
        if (Input.GetMouseButtonDown(0))
        {

        }
        
    }

    private void OnGUI()
    {
        
        if (debug.Length > 0)
        {
            GUI.enabled = true;
            Camera cam = Camera.main;
            Vector3 pos = cam.WorldToScreenPoint(transform.position + Vector3.up * 3);
            GUI.Label(new Rect(pos.x, Screen.height - pos.y, 150, 130), debug);
        }
        
       
    }

    private void OnMouseDown()
    {
        //Debug.Log("WALK");
        /*
        Tile[] t = GameObject.FindObjectsOfType<Tile>();
        foreach(Tile tt in t)
        {
            tt.F = 0;
        }*/
        Agent.selected.moveObjective = this;
        //GameObject.FindObjectOfType<Agent>().moveObjective = this;
        //Soldier.selected.moveObjective = this;
    }

    public void OnMouseExit()
    {
        GetComponent<MeshRenderer>().material.color = Color.clear; //.SetColor("_EmissionColor", Color.green);
        foreach (Tile t in neighbors.Keys)
        {
            t.GetComponent<MeshRenderer>().material.color = Color.clear; //.SetColor("_EmissionColor", Color.red);
        }
    }
}
