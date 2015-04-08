using UnityEngine;
using System.Collections;

public class DungeonCell : MonoBehaviour {
	private IntVector2 _coordinates;

    public IntVector2 Coordinates{ get{ return _coordinates; } set{ _coordinates = value; } }

    public override string ToString()
    {
        return "x: " + _coordinates.x + ",z: " + _coordinates.z;
    }
}
