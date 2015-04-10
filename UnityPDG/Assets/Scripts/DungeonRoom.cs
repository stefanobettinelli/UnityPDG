using UnityEngine;
using System.Collections;

public class DungeonRoom {

    private RoomData data;

    public RoomData Data { get { return data; } set { data = value; } }

    public DungeonRoom()
    {
    }

    public void generateRoomSize(int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        //Debug.Log("generate room size, ranges...: " + minWidth + " " + maxWidth + " " + minHeight + " " + maxHeight + " ");
        int aWidth = Random.Range(minWidth, maxWidth);
        int aHeight = Random.Range(minHeight, maxHeight);
        data = new RoomData(aWidth, aHeight);
    }

    public override string ToString()
    {
        string str = " " + data;
        return str;
    }

}
