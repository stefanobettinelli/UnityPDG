using UnityEngine;
using System.Collections;

public class DungeonCell : MonoBehaviour {
	private IntVector2 _coordinates;
    private WallUnit northWall = null;
    private WallUnit southWall = null;
    private WallUnit eastWall = null;
    private WallUnit westWall = null;

    public void addWallRefenceToCell(WallUnit aWall, string type )
    {
        if (type == "south")
        {
            southWall = aWall;
        }
        else if (type == "north")
        {
            northWall = aWall;
        }
        else if (type == "east")
        {
            eastWall = aWall;
        }
        else if (type == "west")
        {
            westWall = aWall;
        }
        else
            Debug.Log("WRONG WALL TYPE");
    }

    public IntVector2 Coordinates{ get{ return _coordinates; } set{ _coordinates = value; } }

    public override string ToString()
    {
        return "Cell at x: " + _coordinates.x + ",z: " + _coordinates.z;
    }
}
