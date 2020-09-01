using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    public static TileGenerator tg;
    public GameObject tile;
    public int wid, hgt;
    public Tile[,] t;
    // Start is called before the first frame update
    void Awake()
    {
        tg = this;
        t = new Tile[wid, hgt];
        for (int i = 0; i < wid; i++)
        {
            for (int j = 0; j < hgt; j++) {
                   t[i,j] = GameObject.Instantiate(tile, new Vector3(i * 10 + transform.position.x, 0, j * 10 + transform.position.z), Quaternion.identity).GetComponent<Tile>();
                t[i, j].neighbors = new Dictionary<Tile, int>();
                t[i, j].x = i;
                t[i, j].y = j;
            }
        }

        for (int i = 0; i < wid; i++)
        {
            for (int j = 0; j < hgt; j++)
            {
                if(i > 0)
                    t[i, j].neighbors.Add(t[i - 1, j],0);
                if (i < wid - 1)
                    t[i, j].neighbors.Add(t[i + 1, j],0);
                if (j > 0)
                    t[i, j].neighbors.Add(t[i, j - 1],0);
                if (j < hgt - 1)
                    t[i, j].neighbors.Add(t[i, j + 1],0);

                if (i > 0 && j > 0)
                    t[i, j].neighbors.Add(t[i - 1, j - 1],0);
                if (i < wid - 1 && j < hgt - 1)
                    t[i, j].neighbors.Add(t[i + 1, j + 1],0);
                if (j > 0 && i < wid - 1)
                    t[i, j].neighbors.Add(t[i + 1, j - 1],0);
                if (j < hgt - 1 && i > 0)
                    t[i, j].neighbors.Add(t[i - 1, j + 1],0);


            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
