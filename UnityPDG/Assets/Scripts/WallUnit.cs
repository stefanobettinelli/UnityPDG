using UnityEngine;
using System.Collections;

public class WallUnit : MonoBehaviour {

    private DungeonCell cell;

    public void Initialize(DungeonCell cell)
    {
        this.cell = cell;
        transform.parent = cell.transform;
        transform.localPosition = Vector3.zero;
    }
}
