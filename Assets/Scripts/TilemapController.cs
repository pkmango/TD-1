using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{

    private Tilemap map;

    void Start()
    {
        map = GetComponent<Tilemap>();
        //Debug.Log(map.tileAnchor);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
