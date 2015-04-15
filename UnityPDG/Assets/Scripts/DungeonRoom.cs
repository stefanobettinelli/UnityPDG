using UnityEngine;
using System.Collections;

public class DungeonRoom: MonoBehaviour {

    private RoomData data;
    private int minWidth;
    private int maxWidth;
    private int minHeight;
    private int maxHeight;

    public RoomData Data { get { return data; } set { data = value; } }

    public DungeonRoom()
    {
    }

    public DungeonRoom(int minWidth, int maxWidth, int minHeight, int maxHeight)
    {
        generateRoomSize(minWidth, maxWidth, minHeight, maxHeight);
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
