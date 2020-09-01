using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
//Deprecated
public class Soldier : MonoBehaviour
{
    public static Soldier selected;

    public int x, y;
    public int str, acc, agi, end, will;
    public float morale, hp;
    public Animator controller;
    public Tile tile;
    public List<Vector3> takingFire;

    public Tile moveObjective;
    public GameObject targetObjective;

    public SoldierEyes eyes;

    // Start is called before the first frame update
    void Start()
    {
        hp = 100;
        morale = 100;
        timer = UnityEngine.Random.value / 2f + .3f;
        controller = GetComponent<Animator>();
        tile = GameObject.FindObjectOfType<TileGenerator>().t[x, y];
        transform.position = tile.transform.position + Vector3.up * 9;

        eyes = GetComponentInChildren<SoldierEyes>();
    }
    float timer = 0;
    bool jumped = false;
    bool crouched = false;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selected = GameObject.FindObjectsOfType<Soldier>()[0];
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selected = GameObject.FindObjectsOfType<Soldier>()[1];
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selected = GameObject.FindObjectsOfType<Soldier>()[2];
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            selected = GameObject.FindObjectsOfType<Soldier>()[3];
        }

        morale = Mathf.Max(-100, Mathf.Min(100, morale + Time.deltaTime * will));
        if (hp <= 0)
        {
            //die
            
            //controller.enabled = false;
            
            controller.SetTrigger("Die");
            if(controller.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                controller.enabled = false;
                Destroy(GetComponent<Rigidbody>());
                enabled = false;
                GetComponentInChildren<CreateRagDoll>().Break();
            }
           
            //
        }
        else if (morale <= 50)
        {
            if (tile.getCoverValue(takingFire[0]) > 0)
            {
                Crouch();
            }

            // run for cover
            if (morale < 0 || !moveObjective)
                lookForCover();

            // < -25 panic, run away
            if (morale <= -100 && tile.getCoverValue(takingFire[0]) <= 0)
                panic();

            if (morale > 0 && eyes.seen.Count > 0)
                EngageTargets();

            if(path.Count <= 0 || path == null && takingFire.Count > 0)
            {
                transform.LookAt(takingFire[0]);
            }

        }
        else //if morale is > 50 and not dead
        {
            if (hp <= 25)
            {
                //bandage up wound
            }
            else if(ammo <= 0)
            {
                Reload();
            }
            // (timer <= 0)
            {
                Stand();
                takingFire.Clear();
            }
            
            //perform useful action
            if(moveObjective == tile)
            {
                moveObjective = null;
            }
            else if(moveObjective && (path == null || path.Count == 0))
            {
                path = PathFind(moveObjective);
            }

            

        }
        if (path != null && path.Count > 0)
        {
            MoveToTarget();
            firing = false;
        }
        else
        {
            if (targetObjective && eyes.seen.Contains(targetObjective))
            {
                Fire(targetObjective);
            }
            /*
            else if (targetObjective)
            {
                //position yourself to see target
            }
            */
            else if (eyes.seen.Count > 0)
            {
                EngageTargets();
            }
            else
            {
                Idle();
            }
        }

        timer -= Time.deltaTime;
    }

    private void Reload()
    {
        ammo = 30;
        timer = 3;
        controller.SetTrigger("Reload");
        controller.ResetTrigger("Shoot");
        //Reload animation
        //controller.anima
    }

    private void EngageTargets()
    {
        foreach(GameObject go in eyes.seen)
        {
            if (!go.tag.Equals(gameObject.tag))
            {
                targetObjective = go;
                //Fire(go);
                return;
            }
        }
        Idle();
        
    }
    public GameObject bullet;
    public int ammo;
    public bool firing;
    private void Fire(GameObject targetObjective)
    {
        firing = true;
        transform.LookAt(targetObjective.transform);
        controller.SetTrigger("Shoot");
        controller.ResetTrigger("Run");
        controller.ResetTrigger("Jump");
        controller.ResetTrigger("Idle");
        //spawnBullets
        if (timer <= 0)
        {
            if(ammo > 0)
                GameObject.Instantiate(bullet, transform.position + transform.forward * 6, Quaternion.LookRotation(transform.forward) * (Quaternion.RotateTowards(Quaternion.identity, UnityEngine.Random.rotation, 300/acc)));
            ammo--;
            if(ammo <= 0)
            {
                Reload();
            }
            else
                timer = .2f;
        }
        //GetComponentInChildren<Gun>().Fire();
    }

    void Idle()
    {
        controller.SetTrigger("Idle");
        controller.ResetTrigger("Run");
        controller.ResetTrigger("Jump");
        controller.ResetTrigger("Shoot");
    }

    void MoveToTarget()
    {
        if (!jumped)
        {
            controller.SetTrigger("Run");
            controller.ResetTrigger("Idle");
            controller.ResetTrigger("Shoot");
            controller.ResetTrigger("Jump");
        }
        transform.position = Vector3.forward * transform.position.z + Vector3.right * transform.position.x + Vector3.up * (path[0].transform.position.y + 9);
        Vector3 target = path[0].transform.position + Vector3.up * 9;
        Vector3 looktarget = target;
        //
        if (path.Count > 1)
            looktarget = path[1].transform.position + Vector3.up * 9;

        transform.LookAt(target);
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(looktarget - transform.position), Time.deltaTime * 2);
        Vector3 dir = (target - this.transform.position).normalized; //unit length vector towards target
                                                                     //this.transform.position = dir * Time.deltaTime;
        float mult = 1f;
        if (crouched)
            mult = .25f;
        if (path[0].neighbors.ContainsKey(tile) && path[0].neighbors[tile] > 0 && !jumped)
        {
            controller.SetTrigger("Jump");
            controller.ResetTrigger("Run");
            jumped = true;
        }
        if (jumped)
        {
            mult = 0.2f;
        }
        //if(Animacontroller.GetCurrentAnimatorStateInfo().fullPathHash)

        transform.position += dir * Time.deltaTime * agi * mult;
        if (Vector3.Distance(path[0].transform.position + Vector3.up * 9, transform.position) < 1.5f)
        {
            tile = path[0];
            path.RemoveAt(0);
            jumped = false;
        }
    }

    private void panic()
    {
        /*
        try
        {
            if (crouched)
                Stand();
            else
                Crouch();

            if(path.Count == 0 || path.Count > 2)
                path = PathFind(tile.neighbors.Keys.ToArray()[UnityEngine.Random.Range(0, 8)]);
        }catch(Exception e)
        {

        }*/
    }

    private void Crouch()
    {
        controller.SetTrigger("Crouch");
        controller.ResetTrigger("Stand");
        crouched = true;
    }

    private void Stand()
    {
        controller.ResetTrigger("Crouch");
        controller.SetTrigger("Stand");
        crouched = false;
    }

    private List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
    {
        List<Tile> totalPath = new List<Tile>();
        totalPath.Add(current);
        while (cameFrom.ContainsKey(current)) { 
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        //totalPath.Insert(0, tile);
        return totalPath;
     }


    public void lookForCover()
    {
        CoverComparer<Tile> cc = new CoverComparer<Tile>(takingFire);
        SortedSet<Tile> nearby = new SortedSet<Tile>(cc);
        nearby.Add(tile);
        foreach(Tile n in tile.neighbors.Keys)
        {
            if (tile.neighbors[n] < 1000)
            {
                nearby.Add(n);
                foreach (Tile n1 in n.neighbors.Keys)
                {
                    if (n.neighbors[n1] < 1000)
                    {
                        nearby.Add(n1);
                    }
                }
            }
        }
        if (tile != nearby.Max &&(path == null || path.Count == 0 || path[path.Count - 1] != nearby.Max))
            path = PathFind(nearby.Max);
    }

    internal List<Tile> PathFind(Tile dest)
    {
        
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, int> gScore = new Dictionary<Tile, int>();
        gScore[tile] = 0;//:= map with default value of Infinity
        DistComparer<Tile> dc = new DistComparer<Tile>(dest, gScore);
        SortedSet<Tile> open = new SortedSet<Tile>(dc);
        List<Tile> closed = new List<Tile>();
        open.Add(tile);
        tile.F = 0;// dc.fScore.Add(tile, 0);
        int i = 0;
        while (open.Count > 0 && i < 500)
        {
            i++;
            Tile current = open.Min;
            // gScore[current];current.F = open.Count;
            if (current.Equals(dest))
            {
                Debug.Log(i);
                return ReconstructPath(cameFrom, current);
            }
            //current.F = gScore[open.Min];
            //Debug.Log(gScore[open.Min]);
            //Debug.Log(ope)
            if (!open.Remove(current))
            {
                SortedSet<Tile> rectify = new SortedSet<Tile>(dc);
                foreach(Tile t in open)
                {
                    if(!t.Equals(current))
                        rectify.Add(t);
                }
                open = rectify;
            }
            //Debug.Log(open.Remove(current));
            closed.Add(current);
            //open.RemoveWhere(x => x.Equals(current));
            
            foreach (Tile n in current.neighbors.Keys)
            {

                int score = gScore[current] + (int)Vector3.Distance(current.transform.position, n.transform.position) + current.neighbors[n];
                if (score > 1000) continue;
                if (!gScore.ContainsKey(n) || score < gScore[n])
                {
                    // This path to neighbor is better than any previous one. Record it!
                    cameFrom[n] = current;
                    gScore[n] = score;
                    n.F = gScore[n] + (int)Vector3.Distance(n.transform.position, dest.transform.position);

                    if (!open.Contains(n))
                    {
                        open.Add(n);

                    }
                    else
                    {
                        open.Remove(n);
                        open.Add(n);
                    }
                    
                }
                else
                {
                    //dc.fScore[n] = score + (int)Vector3.Distance(n.transform.position, dest.transform.position);

                    //n.F = i;
                }
            }
        }
        

        return null;
    }

    

    public List<Tile> path;
}
