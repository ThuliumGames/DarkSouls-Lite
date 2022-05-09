using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    FORWARD = 0,
    RIGHT,
    BACK,
    LEFT,
    UP,
    DOWN
}

public class RoomNode
{
    public DungeonRoom thisRoom;
    //public DungeonRoom[] connectedRooms;

    public RoomNode(DungeonRoom room)
    {
        thisRoom = room;
        //connectedRooms = new DungeonRoom[room.numOpenDoorways];
    }

    public RoomNode() { }
}

public class GridNode
{
    public Vector2Int mPos;
    public RoomNode thisNode;

    public GridNode(GameObject gm, Vector2Int pos)
    {
        pos = mPos;
        GameObject g = Object.Instantiate(gm, new Vector3(mPos.x * RoomGrid.GRID_SPACING, 0, mPos.y * RoomGrid.GRID_SPACING), Quaternion.identity);
        thisNode = new RoomNode(g.GetComponent<DungeonRoom>());

    }

    public GridNode() { }
    public GridNode(Vector2Int pos) 
    {
        mPos = pos;
    }
}

public static class RoomGrid
{
    public static Dictionary<Vector2Int,GridNode> grid = new Dictionary<Vector2Int, GridNode>();
    public static List<GridNode> gridWithRooms = new List<GridNode>();

    public const int GRID_SPACING = 2;

    public static int minX, minY, maxX, maxY; 

    //Is called by addPiece if it goes out of bounds, changes the bounds and adds blank spaces.
    public static void resizeGrid(Vector2Int pos)
    {
        if (pos.x < minX)
        {
            minX--;
            for (int i = minY; i <= maxY; i++)
            {
                Vector2Int vec = new Vector2Int(minX, i);
                if (vec != pos)
                {
                    grid.Add(pos, new GridNode(pos));
                }
            }
        } else if (pos.x > maxX)
        {
            maxX++;
            for (int i = minY; i <= maxY; i++)
            {
                Vector2Int vec = new Vector2Int(maxX, i);
                if (vec != pos)
                {
                    grid.Add(pos, new GridNode(pos));
                }
            }
        }
        if (pos.y < minY)
        {
            minY--;
            for (int i = minX; i <= maxX; i++)
            {
                Vector2Int vec = new Vector2Int(i, minY);
                if (vec != pos)
                {
                    grid.Add(pos, new GridNode(pos));
                }
            }
        }
        else if (pos.y > maxY)
        {
            maxY++;
            for (int i = minX; i <= maxX; i++)
            {
                Vector2Int vec = new Vector2Int(i, maxY);
                if (vec != pos)
                {
                    grid.Add(pos, new GridNode(pos));
                }
            }
        }
    }

    //Adds piece to both chunks
    public static DungeonRoom addPiece(GameObject gm, Vector2Int pos)
    {
        
        if (grid.ContainsKey(pos))
        {
            grid.Remove(pos);
        }

        Vector2Int thisPos = pos;

        GridNode g = new GridNode(gm, thisPos);
        grid.TryAdd(thisPos, g);
        gridWithRooms.Add(g);
        g.thisNode.thisRoom.generateData();

        if (pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY)
        {
            resizeGrid(pos);
        }

        return g.thisNode.thisRoom;
    }

    public static DungeonRoom getPiece(Vector2Int pos)
    {
        return grid[pos].thisNode.thisRoom;
    }

    //Finds rooms adjacent to point
    public static List<Direction> findAdjacentRooms(Vector2Int pos)
    {
        List<Direction> adjacents = new List<Direction>();
        
        Vector2Int forwardCheck = new Vector2Int(pos.x, pos.y + 1);
        if (grid.ContainsKey(forwardCheck))
        {
            if (gridWithRooms.Contains(grid[forwardCheck]))
            {
                adjacents.Add(Direction.FORWARD);
            }
        }

        Vector2Int rightCheck = new Vector2Int(pos.x + 1, pos.y);
        if (grid.ContainsKey(rightCheck))
        {
            if (gridWithRooms.Contains(grid[rightCheck]))
            {
                adjacents.Add(Direction.FORWARD);
            }
        }

        Vector2Int backCheck = new Vector2Int(pos.x, pos.y - 1);
        if (grid.ContainsKey(backCheck))
        {
            if (gridWithRooms.Contains(grid[backCheck]))
            {
                adjacents.Add(Direction.FORWARD);
            }
        }

        Vector2Int leftCheck = new Vector2Int(pos.x - 1, pos.y);
        if (grid.ContainsKey(leftCheck))
        {
            if (gridWithRooms.Contains(grid[leftCheck]))
            {
                adjacents.Add(Direction.FORWARD);
            }
        }

        return adjacents;
    }

    //Uses BFS to connect open paths
    //public static Vector2Int[] findConnectables() { }

    public static void clearGrid()
    {
        gridWithRooms.Clear();
        grid.Clear();
        minX = minY = maxX = maxY = 0;
    }
}

public class DungeonGenerator : MonoBehaviour
{
    //First in each is the pathway/connector
    public GameObject[] CastleRooms;
    public GameObject[] CatacombsRooms;
    public GameObject[] ConjunctionRooms;

    readonly int numRooms = 9;

    // Called when it's time to generate
    void Generate()
    {
        //startNode = new RoomNode(Instantiate(CastleRooms[1], Vector3.zero, Quaternion.identity).GetComponent<DungeonRoom>());

        DungeonRoom startNode = RoomGrid.addPiece(CastleRooms[1], new Vector2Int(0, 0));

        for (int i = 0; i < numRooms; i++)
        {
            GridNode randomNodeWithRoom = new GridNode();
            RoomNode randomRoom = new RoomNode();
            int numDoors;

            bool validSelection = false;
            do
            {
                randomNodeWithRoom = RoomGrid.gridWithRooms[Random.Range(0, RoomGrid.gridWithRooms.Count)];
                randomRoom = randomNodeWithRoom.thisNode;
                numDoors = randomRoom.thisRoom.unusedDoorways.Length - 1;

                if (randomRoom.thisRoom.numUnusedDoorways > 0)
                {
                    validSelection = true;
                }
            } while (!validSelection);

            Direction doorDirection = new Direction();

            bool validDoorSelection = false;
            do
            {
                int direction = Random.Range(0, numDoors);
                if (randomRoom.thisRoom.unusedDoorways[direction] == true)
                {
                    doorDirection = randomRoom.thisRoom.doorwayDirections[direction];
                    validDoorSelection = true;
                }
                
            } while (!validDoorSelection);



            DungeonRoom dm = new DungeonRoom();
            Vector2Int newPos = new Vector2Int();

            switch (doorDirection)
            {
                case Direction.FORWARD:
                    newPos = new Vector2Int(randomNodeWithRoom.mPos.x, randomNodeWithRoom.mPos.y + 1);
                    dm = RoomGrid.addPiece(CastleRooms[1], newPos);
                    break;
                case Direction.RIGHT:
                    newPos = new Vector2Int(randomNodeWithRoom.mPos.x + 1, randomNodeWithRoom.mPos.y);
                    dm = RoomGrid.addPiece(CastleRooms[1], newPos);
                    break;
                case Direction.BACK:
                    newPos = new Vector2Int(randomNodeWithRoom.mPos.x, randomNodeWithRoom.mPos.y - 1);
                    dm = RoomGrid.addPiece(CastleRooms[1], newPos);
                    break;
                case Direction.LEFT:
                    newPos = new Vector2Int(randomNodeWithRoom.mPos.x - 1, randomNodeWithRoom.mPos.y);
                    dm = RoomGrid.addPiece(CastleRooms[1], newPos);
                    break;
            }

            foreach (Direction d in RoomGrid.findAdjacentRooms(newPos))
            {
                switch (d)
                {
                    case Direction.FORWARD:
                        if (dm.unusedDoorways[0] == true)
                        {
                            DungeonRoom otherDm = randomRoom.thisRoom;
                            otherDm.unusedDoorways[2] = false;
                            otherDm.connectedDoorways[2] = true;
                            otherDm.numUnusedDoorways--;
                            dm.unusedDoorways[0] = false;
                            dm.connectedDoorways[0] = true;
                            dm.numUnusedDoorways--;
                        }
                        break;
                    case Direction.RIGHT:
                        if (dm.unusedDoorways[1] == true)
                        {
                            DungeonRoom otherDm = randomRoom.thisRoom;
                            otherDm.unusedDoorways[3] = false;
                            otherDm.connectedDoorways[3] = true;
                            otherDm.numUnusedDoorways--;
                            dm.unusedDoorways[1] = false;
                            dm.connectedDoorways[1] = true;
                            dm.numUnusedDoorways--;
                        }
                        break;
                    case Direction.BACK:
                        if (dm.unusedDoorways[2] == true)
                        {
                            DungeonRoom otherDm = randomRoom.thisRoom;
                            otherDm.unusedDoorways[0] = false;
                            otherDm.connectedDoorways[0] = true;
                            otherDm.numUnusedDoorways--;
                            dm.unusedDoorways[2] = false;
                            dm.connectedDoorways[2] = true;
                            dm.numUnusedDoorways--;
                        }
                        break;
                    case Direction.LEFT:
                        if (dm.unusedDoorways[3] == true)
                        {
                            DungeonRoom otherDm = randomRoom.thisRoom;
                            otherDm.unusedDoorways[1] = false;
                            otherDm.connectedDoorways[1] = true;
                            otherDm.numUnusedDoorways--;
                            dm.unusedDoorways[3] = false;
                            dm.connectedDoorways[3] = true;
                            dm.numUnusedDoorways--;
                        }
                        break;
                }
            }

            
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Generate();
        }
    }
}
