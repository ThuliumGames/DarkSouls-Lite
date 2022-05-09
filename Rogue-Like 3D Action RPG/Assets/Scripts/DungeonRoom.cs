using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRoom : MonoBehaviour
{
    [HideInInspector]
    public int numUnusedDoorways;

    //Uses enum direction rules. 0 is forward, 1 is right, etc.
    public bool[] unusedDoorways;
    public bool[] connectedDoorways;

    public bool hasTop;
    public bool hasBottom;

    //Door position in local space
    public float[] doorwayPositions;
    public Direction[] doorwayDirections;

    //Use for room parameters.
    public float maxWidth, maxHeight, maxDepth;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void generateData()
    {
        numUnusedDoorways = unusedDoorways.Length;
        foreach (bool b in unusedDoorways)
        {
            if (b == false)
            {
                numUnusedDoorways--;
            }
        }
    }

    public void generateInterior()
    {

    }
}
