using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour, Shootable
{

    public static Agent selected;

    private void OnMouseDown()
    {
        selected = this;
    }

    public Animator legs; // for moving the legs
    public Animator body; //for moving the arms/head
    public CreateRagDoll rags; // for breaking into a ragdoll on death

    public Transform bodyFacing; //for looking at something with the chest up

    public int str, // helps manage gun recoil and carry heavier things
        acc, // helps with gun accuracy
        agi, //helps with base decision making speed
        end, // helps health points and running farther
        will, // regenerates morale and resists psychic attacks
        intl; // intellect, controls various other things like healing and other actions
 
    public float hp, //health - based off strength and endurance, if this reaches 0 the character dies
        morale, // morale - regenerates based off will. if reaches 50 the character looks for cover. at 0 they are suppressed. at -100 they panic
        stamina; //stamina - based off strength, regenerates based off endurance, helps sprint.

    public Tile tile; // which tile they are occupiying
    public int x, y; //where the soldier is located on the tilegrid

    public Tile moveObjective; // where the commander told them to move
    public GameObject targetObjective; // which target the commander told them to prioritize
    public GameObject engagedTarget; // which target out of the ones they see are they engaging

    public List<Vector3> takingFire; //where the agent is being shot from.

    public Gun gun;

    public SoldierEyes eyes;

    public void getShot(Damage d)
    {
        hp -= d.blunt;
        hp -= d.electric;
        hp -= d.thermal;
        hp -= d.piercing;
    }

    // Start is called before the first frame update
    void Start()
    {
        tile = GameObject.FindObjectOfType<TileGenerator>().t[x, y];
        tile.occupied = this;
        transform.position = tile.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        morale = Mathf.Max(-100, Mathf.Min(100, morale + will * Time.deltaTime));

        stamina = Mathf.Max(0, Mathf.Min(100, stamina + end * Time.deltaTime));


        if (hp <= 0)
        {
            Die();
        }
        else if(morale > 50)
        {
            // optimal decision making
            MakeDecision();
        }
        else if(morale > -50)
        {
            int cover = tile.getCoverValue(takingFire[0]);

            if (takingFire.Count > 0 && cover <= 0)
            {
                morale -= takingFire.Count * Time.deltaTime * 10;
            }

            // take cover we're bring shot - return fire if able
            if (cover > 0)
            {
                Crouch();
                MakeDecision();
            }
            else
            {
                lookForCover();
            }
        }
        else if(morale > -100)
        {
            if (tile.getCoverValue(takingFire[0]) > 3)
            {
                Crouch();
            }
            else
            {
                // take cover - suppressed
                lookForCover();
            }
        }
        else
        {
            // panic!
            Panic();
        }


        //Goal Achievement
        if (path != null && path.Count > 0)
        {
            Center();
            bodyFacing.transform.forward = transform.forward;
            MoveToTarget();
            if (firing)
            {
                firing = false;
                Aim();
            }
            
        }
        else
        {
            Vector3 preserveFacing = bodyFacing.forward;
            transform.forward = Vector3.Lerp(transform.forward,bodyFacing.forward.x * Vector3.right + bodyFacing.forward.z * Vector3.forward + transform.forward.y * Vector3.up,Time.deltaTime);
            bodyFacing.forward = preserveFacing;
            //transform.rotation = Quaternion.Slerp(transform.rotation, bodyFacing.rotation, Time.deltaTime);

            Lean();
            if (gun.loading)
            {
                Reload();
            }
            else if (engagedTarget && eyes.seen.Contains(engagedTarget))
            {
                Aim();
                Vector3 toTarget = engagedTarget.transform.position - transform.position;
                Vector3 gunTo = gun.barrel.transform.forward;
                if(Vector3.Angle(toTarget,gunTo) < 25)
                    Fire(engagedTarget);
            }
            else if (eyes.seen.Count > 0)
            {
                EngageTargets();
            }
            else
            {
                Idle();
            }
        }
    }

    private void Center()
    {
        legs.SetTrigger("Center");
        legs.ResetTrigger("Right");
        legs.ResetTrigger("Left");
    }

    private void Lean()
    {

        try
        {
            if(eyes.Blocked())
            {
                int x = (int)Mathf.Round(transform.forward.x);
                int y = (int)Mathf.Round(transform.forward.z);
                Debug.DrawLine(bodyFacing.transform.position, bodyFacing.transform.position + x * Vector3.right * 10 + y * Vector3.forward * 10, Color.magenta);

                if (tile.neighbors[TileGenerator.tg.t[tile.x + x, tile.y + y]] > 0)
                {
                    int xr = (int)Mathf.Round(transform.right.x);
                    int yr = (int)Mathf.Round(transform.right.z);

                    Tile leanL = TileGenerator.tg.t[tile.x + xr, tile.y + yr];
                    Tile leanR = TileGenerator.tg.t[tile.x - xr, tile.y - yr];
                    Tile leanLF = TileGenerator.tg.t[tile.x + xr + x, tile.y + yr + y];
                    Tile leanRF = TileGenerator.tg.t[tile.x - xr + x, tile.y - yr + y];
                    if (tile.neighbors[leanR] <= 0 && leanR.neighbors[leanRF] <= 0)
                    {
                        legs.SetTrigger("Right");
                        legs.ResetTrigger("Center");
                        legs.ResetTrigger("Left");
                    }
                    else if (tile.neighbors[leanL] <= 0 && leanL.neighbors[leanLF] <= 0)
                    {
                        legs.ResetTrigger("Right");
                        legs.ResetTrigger("Center");
                        legs.SetTrigger("Left");
                    }


                }

            }
        }catch (Exception e)
        {

        }
        /*
        if (tile.Obstructed(bodyFacing.forward))
        {
            legs.SetTrigger("Right");
        }*/
        
    }

    public float timer;
    private void MakeDecision()
    {
        if(timer <= 0)
        {
            
            if (hp <= 25)
            {
                //bandage up wound
                //Heal();
            }
            else if (gun.ammo <= 0)
            {
                Reload();
            }

            if(morale > 50)
            {
                Stand();
                takingFire.Clear();
            }

            //perform useful action
            if (moveObjective == tile)
            {
                moveObjective = null;
            }
            else if (moveObjective && (path == null || path.Count == 0))
            {
                path = PathFind(moveObjective);
            }
            
            if (targetObjective && eyes.seen.Contains(targetObjective))
            {
                engagedTarget = targetObjective;
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

            timer = 30 / agi;
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }

    private void Heal()
    {
        throw new NotImplementedException();
    }

    public void Die()
    {
        legs.enabled = false;
        body.enabled = false;
        Destroy(GetComponent<Rigidbody>());
        enabled = false;
        rags.Break();
        
    }

    void Idle()
    {
        legs.ResetTrigger("Run");
        legs.ResetTrigger("Jump");
        legs.SetTrigger("Idle");
        body.ResetTrigger("Aim");
        body.ResetTrigger("Fire");
        body.ResetTrigger("Reload");
        body.SetTrigger("Idle");
        transform.position = Vector3.Lerp(transform.position, tile.transform.position, Time.deltaTime);
    }

    void Run()
    {
        legs.ResetTrigger("Jump");
        legs.ResetTrigger("Idle");
        legs.SetTrigger("Run");
    }

    void Jump()
    {
        legs.ResetTrigger("Run");
        legs.ResetTrigger("Idle");
        legs.SetTrigger("Jump");
        jumped = true;
        //body jump animation
    }

    public bool jumped, crouched;

    public List<Tile> path;

    void MoveToTarget()
    {
        if (!jumped)
        {
            Run();
        }
        transform.position = Vector3.forward * transform.position.z + Vector3.right * transform.position.x + Vector3.up * (path[0].transform.position.y);
        Vector3 target = path[0].transform.position;
        Vector3 looktarget = target;
        if (path.Count > 1)
            looktarget = path[1].transform.position;

        transform.LookAt(target);
        Vector3 dir = (target - this.transform.position).normalized; //unit length vector towards target
                                                                     //this.transform.position = dir * Time.deltaTime;
        float mult = 1f;
        if (crouched)
            mult = .25f;
        if (path[0].neighbors.ContainsKey(tile) && path[0].neighbors[tile] > 0 && !jumped)
        {
            Jump();
        }
        if (jumped)
        {
            mult = 0.2f;
        }
        transform.position += dir * Time.deltaTime * agi * mult;
        if (Vector3.Distance(path[0].transform.position, transform.position) < 1.5f)
        {
            tile.occupied = null;
            tile = path[0];
            tile.occupied = this;
            x = tile.x;
            y = tile.y;
            path.RemoveAt(0);
            jumped = false;
        }
    }

    private void Panic()
    {
        try
        {
            if (crouched)
                Stand();
            else
                Crouch();

            if (path.Count == 0 || path == null || moveObjective)
                moveObjective = TileGenerator.tg.t[x + UnityEngine.Random.Range(-5, 5), y + UnityEngine.Random.Range(-5, 5)];
        }
        catch (Exception e)
        {

        }
       
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
        legs.SetTrigger("Crouch");
        legs.ResetTrigger("Stand");
        crouched = true;
    }

    private void Stand()
    {
        legs.ResetTrigger("Crouch");
        legs.SetTrigger("Stand");
        crouched = false;
    }

    private void Reload()
    { 
        gun.Load();
        body.SetTrigger("Reload");
        body.ResetTrigger("Fire");
        body.ResetTrigger("Idle");
        body.ResetTrigger("Aim");
    }

    private void EngageTargets()
    {
        GameObject close = null;
        foreach (GameObject go in eyes.seen)
        {
            if (!go.tag.Equals(gameObject.tag))
            {
                if (close == null || Vector3.Distance(go.transform.position, transform.position) < Vector3.Distance(close.transform.position, transform.position))
                    close = go;
            }
        }
        engagedTarget = close;
    }
    
    private void Aim()
    {
        Vector3 pos = engagedTarget.GetComponentInChildren<CenterMass>().transform.position;
        bodyFacing.LookAt(pos,Vector3.up);
        gun.barrel.transform.forward = pos - gun.barrel.transform.position;
        body.SetTrigger("Aim");
        body.ResetTrigger("Fire");
        body.ResetTrigger("Idle");
        body.ResetTrigger("Reload");
    }

    public bool firing;

    private void Fire(GameObject targetObjective)
    {
        gun.Fire(acc);
        body.SetTrigger("Fire");
        body.ResetTrigger("Aim");
        body.ResetTrigger("Idle");
        body.ResetTrigger("Reload");
        firing = true;
    }

    private List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
    {
        List<Tile> totalPath = new List<Tile>();
        totalPath.Add(current);
        while (cameFrom.ContainsKey(current))
        {
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
        foreach (Tile n in tile.neighbors.Keys)
        {
            if (tile.neighbors[n] < 1000 && n.occupied == null)
            {
                nearby.Add(n);
                foreach (Tile n1 in n.neighbors.Keys)
                {
                    if (n.neighbors[n1] < 1000 && n1.occupied == null)
                    {
                        nearby.Add(n1);
                    }
                }
            }
        }
        if (tile != nearby.Max && (path == null || path.Count == 0 || path[path.Count - 1] != nearby.Max))
            path = PathFind(nearby.Max);
    }

    internal List<Tile> PathFind(Tile dest)
    {
        //Debug.Log("PATHFINDING:");
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        Dictionary<Tile, int> gScore = new Dictionary<Tile, int>();
        gScore[tile] = 0;//:= map with default value of Infinity
        DistComparer<Tile> dc = new DistComparer<Tile>(dest, gScore);
        SortedSet<Tile> open = new SortedSet<Tile>(dc);
        List<Tile> closed = new List<Tile>();
        open.Add(tile);
        tile.F = 0;// dc.fScore.Add(tile, 0);
        int i = 0;
        //Debug.Log(open.Count);
        while (open.Count > 0 && i < 500)
        {
            tile.debug = "" + i;
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
            //Debug.Log(ope);
            if (!open.Remove(current))
            {
                SortedSet<Tile> rectify = new SortedSet<Tile>(dc);
                foreach (Tile t in open)
                {
                    if (!t.Equals(current))
                        rectify.Add(t);
                }
                open = rectify;
            }
            //Debug.Log(open.Remove(current));
            closed.Add(current);
            //open.RemoveWhere(x => x.Equals(current));

            Debug.Log(current.neighbors.Keys.Count);
            foreach (Tile n in current.neighbors.Keys)
            {
                
              
                int score = gScore[current] + (int)Vector3.Distance(current.transform.position, n.transform.position) + current.neighbors[n];
                //n.debug = score + "!";
                if (score > 1000 || n.occupied) continue;


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
            }
        }


        return null;
    }
}


public interface Shootable
{
    void getShot(Damage d);
}
[System.Serializable]
public struct Damage{
    public int piercing, blunt, electric, thermal;
}


public class DistComparer<T> : IComparer<T>
{
    //public Dictionary<Tile, int> fScore;
    Tile dest;
    public DistComparer(Tile dest, Dictionary<Tile, int> gScore)
    {
        this.dest = dest;
        //fScore = new Dictionary<Tile, int>();
    }

    public int Compare(T x, T y)
    {

        Tile a = x as Tile;
        Tile b = y as Tile;
        if (a == b) return 0;
        int val = a.F - b.F;
        if (val == 0) return a.GetInstanceID() - b.GetInstanceID();
        return val;
    }
}

public class CoverComparer<T> : IComparer<T>
{
    List<Vector3> shotFrom;
    public CoverComparer(List<Vector3> shotFrom)
    {
        this.shotFrom = shotFrom;
        //fScore = new Dictionary<Tile, int>();
    }


    public int Compare(T x, T y)
    {

        Tile a = x as Tile;
        Tile b = y as Tile;
        int vala = 0;
        int valb = 0;
        foreach (Vector3 shooter in shotFrom)
        {
            vala += a.getCoverValue(shooter);
            valb += b.getCoverValue(shooter);
        }
        vala /= shotFrom.Count;
        valb /= shotFrom.Count;
        a.debug = vala + " ";
        b.debug = valb + " ";

        return vala - valb;
    }
}
