using UnityEngine;
using System.Collections;

public class DungeonCell : MonoBehaviour {
	private IntVector2 _coordinates;
    private WallUnit northWall = null;
    private string northWalls = "none";
    private WallUnit southWall = null;
    private string southWalls = "none";
    private WallUnit eastWall = null;
    private string eastWalls = "none";
    private WallUnit westWall = null;
    private string westWalls = "none";

    public void addWallRefenceToCell(WallUnit aWall, string type )
    {
        if (type == "south")
        {
            southWall = aWall;
            southWalls = "south";
        }
        else if (type == "north")
        {
            northWall = aWall;
            northWalls = "north";
        }
        else if (type == "east")
        {
            eastWall = aWall;
            eastWalls = "east";
        }
        else if (type == "west")
        {
            westWall = aWall;
            westWalls = "west";
        }
        else
            Debug.Log("WRONG WALL TYPE");
    }

    public IntVector2 Coordinates{ get{ return _coordinates; } set{ _coordinates = value; } }

    public bool destroyWall(string type)
    {
        if (type == "south" && southWall!=null)
        {
            DestroyImmediate(southWall.gameObject);
            //print("SOTH WALL DESTROYED");
            southWall = null;
            return true;
        }
        else if (type == "north" && northWall!=null)
        {
            DestroyImmediate(northWall.gameObject);
            //print("NORTH WALL DESTROYED");
            northWall = null;
            return true;
        }
        else if (type == "east" && eastWall!=null)
        {
            DestroyImmediate(eastWall.gameObject);
            //print("EAST WALL DESTROYED");
            eastWall = null;
            return true;
        }
        else if (type == "west" && westWall!=null)
        {
            DestroyImmediate(westWall.gameObject);
            //print("WEST WALL DESTROYED");
            westWall = null;
            return true;
        }
        else
            return false;
    }

    public override string ToString()
    {
        return "Cell at x: " + _coordinates.x + ",z: " + _coordinates.z + ", Walls -> " + northWalls + ", " + eastWalls + ", " + southWalls + ", " + westWalls;
    }
}
